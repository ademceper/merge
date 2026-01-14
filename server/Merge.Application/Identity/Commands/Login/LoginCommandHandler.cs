using MediatR;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Merge.Application.DTOs.Auth;
using Merge.Application.DTOs.User;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using UserEntity = Merge.Domain.Modules.Identity.User;
using RefreshTokenEntity = Merge.Domain.Modules.Identity.RefreshToken;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using Merge.Domain.SharedKernel.DomainEvents;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Identity.Commands.Login;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 12.1: Magic Number Sorunu - Configuration kullanımı
public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly SignInManager<UserEntity> _signInManager;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;
    private readonly SecuritySettings _securitySettings;
    private readonly IMapper _mapper;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        UserManager<UserEntity> userManager,
        SignInManager<UserEntity> signInManager,
        IDbContext context,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettings,
        IOptions<SecuritySettings> securitySettings,
        IMapper mapper,
        ILogger<LoginCommandHandler> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
        _securitySettings = securitySettings.Value;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt. Email: {Email}", request.Email);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Failed login attempt - user not found. Email: {Email}", request.Email);
            throw new BusinessException("Email veya şifre hatalı.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out. Email: {Email}", request.Email);
                throw new BusinessException("Hesabınız geçici olarak kilitlendi. Lütfen daha sonra tekrar deneyin.");
            }
            _logger.LogWarning("Failed login attempt. Email: {Email}", request.Email);
            throw new BusinessException("Email veya şifre hatalı.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        // ✅ ARCHITECTURE: User entity'sini context'e attach et (domain event'lerin track edilmesi için)
        // UserManager kendi context'ini kullanıyor, bu yüzden bizim context'imizde User entity'si track edilmiyor
        // Domain event'lerin OutboxMessage'lar olarak kaydedilmesi için User entity'sini context'e attach etmeliyiz
        var userEntry = _context.Users.Attach(user);
        userEntry.State = Microsoft.EntityFrameworkCore.EntityState.Unchanged; // Sadece track et, değişiklik yapma

        _logger.LogInformation(
            "User logged in successfully. UserId: {UserId}, Email: {Email}",
            user.Id, request.Email);

        return await GenerateAuthResponseAsync(user, roles, request.IpAddress, cancellationToken);
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(UserEntity user, IList<string> roles, string? ipAddress, CancellationToken cancellationToken)
    {
        var accessToken = await GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        var (refreshToken, plainToken) = GenerateRefreshToken(user.Id, ipAddress);
        
        // ✅ BOLUM 1.5: Domain Events - UserLoggedInEvent (User aggregate root üzerinde)
        user.AddDomainEvent(new UserLoggedInEvent(
            user.Id,
            user.Email ?? string.Empty,
            ipAddress));
        
        _context.Set<RefreshTokenEntity>().Add(refreshToken);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

               var userDto = _mapper.Map<UserDto>(user);
               // ✅ BOLUM 12.1: Magic Number Sorunu - Configuration kullanımı
               // ✅ BOLUM 4.2: Record DTOs - Immutable record, with expression kullan
               userDto = userDto with { Role = roles.Count > 0 ? roles[0] : _securitySettings.DefaultUserRole };

        return new AuthResponseDto(
            Token: accessToken,
            ExpiresAt: expiresAt,
            RefreshToken: plainToken,
            RefreshTokenExpiresAt: refreshToken.ExpiresAt,
            User: userDto);
    }

    private (RefreshTokenEntity, string) GenerateRefreshToken(Guid userId, string? ipAddress = null)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();


        rng.GetBytes(randomBytes);

        var plainToken = Convert.ToBase64String(randomBytes);
        var tokenHash = TokenHasher.HashToken(plainToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var refreshToken = RefreshTokenEntity.Create(
            userId,
            tokenHash,
            DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            ipAddress);

        return (refreshToken, plainToken);
    }

    private async Task<string> GenerateJwtToken(UserEntity user)
    {
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Key ?? throw new InvalidOperationException("JWT Key bulunamadı"));

        var roles = await _userManager.GetRolesAsync(user);
        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU) - 6 base claim + roles count
        var claims = new List<Claim>(6 + roles.Count)
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
