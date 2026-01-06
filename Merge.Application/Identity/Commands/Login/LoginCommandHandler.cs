using MediatR;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Merge.Application.DTOs.Auth;
using Merge.Application.DTOs.User;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Common;
using Merge.Domain.Entities;
using UserEntity = Merge.Domain.Entities.User;

namespace Merge.Application.Identity.Commands.Login;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly SignInManager<UserEntity> _signInManager;
    private readonly IDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly ILogger<LoginCommandHandler> _logger;

    // ✅ SECURITY: Access token süresi 15 dakika
    private const int AccessTokenExpirationMinutes = 15;
    // ✅ SECURITY: Refresh token süresi 7 gün
    private const int RefreshTokenExpirationDays = 7;

    public LoginCommandHandler(
        UserManager<UserEntity> userManager,
        SignInManager<UserEntity> signInManager,
        IDbContext context,
        IConfiguration configuration,
        IMapper mapper,
        ILogger<LoginCommandHandler> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _configuration = configuration;
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

        _logger.LogInformation(
            "User logged in successfully. UserId: {UserId}, Email: {Email}",
            user.Id, request.Email);

        return await GenerateAuthResponseAsync(user, roles, null, cancellationToken);
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(UserEntity user, IList<string> roles, string? ipAddress, CancellationToken cancellationToken)
    {
        var accessToken = await GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes);

        var (refreshToken, plainToken) = GenerateRefreshToken(user.Id, ipAddress);
        _context.Set<RefreshToken>().Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Role = roles.Count > 0 ? roles[0] : "Customer";

        return new AuthResponseDto
        {
            Token = accessToken,
            ExpiresAt = expiresAt,
            RefreshToken = plainToken,
            RefreshTokenExpiresAt = refreshToken.ExpiresAt,
            User = userDto
        };
    }

    private (RefreshToken, string) GenerateRefreshToken(Guid userId, string? ipAddress = null)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var plainToken = Convert.ToBase64String(randomBytes);
        var tokenHash = TokenHasher.HashToken(plainToken);

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays),
            CreatedByIp = ipAddress
        };

        return (refreshToken, plainToken);
    }

    private async Task<string> GenerateJwtToken(UserEntity user)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key bulunamadı"));

        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
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
            Expires = DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
