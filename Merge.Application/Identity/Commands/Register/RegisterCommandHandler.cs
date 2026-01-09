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
using Merge.Domain.Common.DomainEvents;
using UserEntity = Merge.Domain.Entities.User;

namespace Merge.Application.Identity.Commands.Register;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 12.1: Magic Number Sorunu - Configuration kullanımı
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;
    private readonly SecuritySettings _securitySettings;
    private readonly IMapper _mapper;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        UserManager<UserEntity> userManager,
        IDbContext context,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettings,
        IOptions<SecuritySettings> securitySettings,
        IMapper mapper,
        ILogger<RegisterCommandHandler> logger)
    {
        _userManager = userManager;
        _context = context;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
        _securitySettings = securitySettings.Value;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registration attempt. Email: {Email}", request.Email);

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Registration failed - email already exists. Email: {Email}", request.Email);
            throw new BusinessException("Bu email adresi zaten kullanılıyor.");
        }

        var user = new UserEntity
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            _logger.LogWarning("Registration failed - validation errors. Email: {Email}, Errors: {Errors}", request.Email, errors);
            throw new ValidationException("Kullanıcı oluşturulamadı.", errors);
        }

        // ✅ BOLUM 12.1: Magic Number Sorunu - Configuration kullanımı
        // Default role assignment
        await _userManager.AddToRoleAsync(user, _securitySettings.DefaultUserRole);

        var roles = await _userManager.GetRolesAsync(user);

        // ✅ ARCHITECTURE: User entity'sini context'e attach et (domain event'lerin track edilmesi için)
        // UserManager kendi context'ini kullanıyor, bu yüzden bizim context'imizde User entity'si track edilmiyor
        // Domain event'lerin OutboxMessage'lar olarak kaydedilmesi için User entity'sini context'e attach etmeliyiz
        var userEntry = _context.Users.Attach(user);
        userEntry.State = Microsoft.EntityFrameworkCore.EntityState.Unchanged; // Sadece track et, değişiklik yapma

        _logger.LogInformation(
            "User registered successfully. UserId: {UserId}, Email: {Email}",
            user.Id, request.Email);

        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        return await GenerateAuthResponseAsync(user, roles, request.IpAddress, cancellationToken);
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(UserEntity user, IList<string> roles, string? ipAddress, CancellationToken cancellationToken)
    {
        var accessToken = await GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        var (refreshToken, plainToken) = GenerateRefreshToken(user.Id, ipAddress);
        
        // ✅ BOLUM 1.5: Domain Events - UserRegisteredEvent (User aggregate root üzerinde)
        user.AddDomainEvent(new UserRegisteredEvent(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            ipAddress));
        
        _context.Set<RefreshToken>().Add(refreshToken);
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

    private (RefreshToken, string) GenerateRefreshToken(Guid userId, string? ipAddress = null)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var plainToken = Convert.ToBase64String(randomBytes);
        var tokenHash = TokenHasher.HashToken(plainToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var refreshToken = RefreshToken.Create(
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

