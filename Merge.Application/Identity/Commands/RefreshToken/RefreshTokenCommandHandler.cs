using MediatR;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
using UserEntity = Merge.Domain.Entities.User;
using RefreshTokenEntity = Merge.Domain.Entities.RefreshToken;

namespace Merge.Application.Identity.Commands.RefreshToken;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 12.1: Magic Number Sorunu - Configuration kullanımı
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;
    private readonly SecuritySettings _securitySettings;
    private readonly IMapper _mapper;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        UserManager<UserEntity> userManager,
        IDbContext context,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettings,
        IOptions<SecuritySettings> securitySettings,
        IMapper mapper,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _userManager = userManager;
        _context = context;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
        _securitySettings = securitySettings.Value;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Token refresh attempt. RefreshToken: {RefreshToken}", request.RefreshToken);

        // ✅ BOLUM 9.1: Refresh token hash'lenmiş olarak saklanıyor
        var tokenHash = TokenHasher.HashToken(request.RefreshToken);
        var refreshTokenEntity = await _context.Set<RefreshTokenEntity>()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (refreshTokenEntity == null)
        {
            _logger.LogWarning("Token refresh failed - invalid token. RefreshToken: {RefreshToken}", request.RefreshToken);
            throw new BusinessException("Geçersiz refresh token.");
        }

        if (!refreshTokenEntity.IsActive)
        {
            _logger.LogWarning("Token refresh failed - token expired or revoked. RefreshToken: {RefreshToken}", request.RefreshToken);
            throw new BusinessException("Refresh token geçersiz veya süresi dolmuş.");
        }

        var user = refreshTokenEntity.User;
        var roles = await _userManager.GetRolesAsync(user);

        // ✅ SECURITY: Yeni refresh token oluştur (rotation)
        var (newRefreshToken, plainToken) = GenerateRefreshToken(user.Id, request.IpAddress);

        // ✅ SECURITY: Eski refresh token'ı revoke et (Domain Method kullanımı)
        refreshTokenEntity.Revoke(request.IpAddress, newRefreshToken.TokenHash);

        _context.Set<RefreshTokenEntity>().Add(newRefreshToken);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Token refreshed successfully. UserId: {UserId}",
            user.Id);

        var accessToken = await GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

               var userDto = _mapper.Map<UserDto>(user);
               // ✅ BOLUM 12.1: Magic Number Sorunu - Configuration kullanımı
               // ✅ BOLUM 4.2: Record DTOs - Immutable record, with expression kullan
               userDto = userDto with { Role = roles.Count > 0 ? roles[0] : _securitySettings.DefaultUserRole };

        return new AuthResponseDto(
            Token: accessToken,
            ExpiresAt: expiresAt,
            RefreshToken: plainToken,
            RefreshTokenExpiresAt: newRefreshToken.ExpiresAt,
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

