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
using UserEntity = Merge.Domain.Modules.Identity.User;
using RefreshTokenEntity = Merge.Domain.Modules.Identity.RefreshToken;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Identity.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    UserManager<UserEntity> userManager,
    IDbContext context,
    IUnitOfWork unitOfWork,
    IOptions<JwtSettings> jwtSettings,
    IOptions<SecuritySettings> securitySettings,
    IMapper mapper,
    ILogger<RefreshTokenCommandHandler> logger) : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Token refresh attempt. RefreshToken: {RefreshToken}", request.RefreshToken);

        var tokenHash = TokenHasher.HashToken(request.RefreshToken);
        var refreshTokenEntity = await context.Set<RefreshTokenEntity>()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (refreshTokenEntity == null)
        {
            logger.LogWarning("Token refresh failed - invalid token. RefreshToken: {RefreshToken}", request.RefreshToken);
            throw new BusinessException("Geçersiz refresh token.");
        }

        if (!refreshTokenEntity.IsActive)
        {
            logger.LogWarning("Token refresh failed - token expired or revoked. RefreshToken: {RefreshToken}", request.RefreshToken);
            throw new BusinessException("Refresh token geçersiz veya süresi dolmuş.");
        }

        var user = refreshTokenEntity.User;
        var roles = await userManager.GetRolesAsync(user);

        var (newRefreshToken, plainToken) = GenerateRefreshToken(user.Id, request.IpAddress);

        refreshTokenEntity.Revoke(request.IpAddress, newRefreshToken.TokenHash);

        context.Set<RefreshTokenEntity>().Add(newRefreshToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Token refreshed successfully. UserId: {UserId}",
            user.Id);

        var accessToken = await GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.Value.AccessTokenExpirationMinutes);

        var userDto = mapper.Map<UserDto>(user);
        userDto = userDto with { Role = roles.Count > 0 ? roles[0] : securitySettings.Value.DefaultUserRole };

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

        var refreshToken = RefreshTokenEntity.Create(
            userId,
            tokenHash,
            DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpirationDays),
            ipAddress);

        return (refreshToken, plainToken);
    }

    private async Task<string> GenerateJwtToken(UserEntity user)
    {
        var key = Encoding.UTF8.GetBytes(jwtSettings.Value.Key ?? throw new InvalidOperationException("JWT Key bulunamadı"));

        var roles = await userManager.GetRolesAsync(user);
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
            Expires = DateTime.UtcNow.AddMinutes(jwtSettings.Value.AccessTokenExpirationMinutes),
            Issuer = jwtSettings.Value.Issuer,
            Audience = jwtSettings.Value.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

