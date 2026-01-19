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
using Merge.Application.DTOs.Identity;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using UserEntity = Merge.Domain.Modules.Identity.User;
using RefreshTokenEntity = Merge.Domain.Modules.Identity.RefreshToken;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketplace;
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
        logger.LogInformation("Token refresh attempt. RefreshToken: {RefreshToken}", LogMasking.MaskToken(request.RefreshToken));

        var tokenHash = TokenHasher.HashToken(request.RefreshToken);
        var refreshTokenEntity = await context.Set<RefreshTokenEntity>()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (refreshTokenEntity is null)
        {
            logger.LogWarning("Token refresh failed - invalid token. RefreshToken: {RefreshToken}", LogMasking.MaskToken(request.RefreshToken));
            throw new BusinessException("Geçersiz refresh token.");
        }

        if (!refreshTokenEntity.IsActive)
        {
            logger.LogWarning("Token refresh failed - token expired or revoked. RefreshToken: {RefreshToken}", LogMasking.MaskToken(request.RefreshToken));
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
        userDto = userDto with { Role = roles.FirstOrDefault() ?? securitySettings.Value.DefaultUserRole };

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
        var key = Encoding.UTF8.GetBytes(jwtSettings.Value.Key ?? throw new ConfigurationException("JWT Key bulunamadı"));

        var roles = await userManager.GetRolesAsync(user);
        
        // Get all roles and permissions
        var rolesAndPermissions = await GetUserRolesAndPermissionsAsync(user.Id);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Platform roles
        foreach (var role in rolesAndPermissions.PlatformRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Store roles
        foreach (var storeRole in rolesAndPermissions.StoreRoles)
        {
            claims.Add(new Claim("store_role", $"{storeRole.StoreId}:{storeRole.RoleName}"));
        }

        // Organization roles
        foreach (var orgRole in rolesAndPermissions.OrganizationRoles)
        {
            claims.Add(new Claim("org_role", $"{orgRole.OrganizationId}:{orgRole.RoleName}"));
        }

        // Store customer roles
        foreach (var storeCustomerRole in rolesAndPermissions.StoreCustomerRoles)
        {
            claims.Add(new Claim("store_customer_role", $"{storeCustomerRole.StoreId}:{storeCustomerRole.RoleName}"));
        }

        // Permissions
        foreach (var permission in rolesAndPermissions.Permissions)
        {
            claims.Add(new Claim("permission", permission));
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

    private async Task<UserRolesAndPermissionsDto> GetUserRolesAndPermissionsAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return new UserRolesAndPermissionsDto([], [], [], [], []);
        }

        var platformRoles = await userManager.GetRolesAsync(user);

        var storeRoles = await context.Set<StoreRole>()
            .AsNoTracking()
            .Include(sr => sr.Store)
            .Include(sr => sr.Role)
            .Where(sr => sr.UserId == userId && !sr.IsDeleted)
            .Select(sr => new StoreRoleInfo(
                sr.StoreId,
                sr.Store.StoreName,
                sr.Role.Name ?? string.Empty,
                sr.RoleId))
            .ToListAsync();

        var organizationRoles = await context.Set<OrganizationRole>()
            .AsNoTracking()
            .Include(or => or.Organization)
            .Include(or => or.Role)
            .Where(or => or.UserId == userId && !or.IsDeleted)
            .Select(or => new OrganizationRoleInfo(
                or.OrganizationId,
                or.Organization.Name,
                or.Role.Name ?? string.Empty,
                or.RoleId))
            .ToListAsync();

        var storeCustomerRoles = await context.Set<StoreCustomerRole>()
            .AsNoTracking()
            .Include(scr => scr.Store)
            .Include(scr => scr.Role)
            .Where(scr => scr.UserId == userId && !scr.IsDeleted)
            .Select(scr => new StoreCustomerRoleInfo(
                scr.StoreId,
                scr.Store.StoreName,
                scr.Role.Name ?? string.Empty,
                scr.RoleId))
            .ToListAsync();

        var roleIds = new List<Guid>();

        var platformRoleEntities = await context.Roles
            .AsNoTracking()
            .Where(r => platformRoles.Contains(r.Name ?? string.Empty))
            .Select(r => r.Id)
            .ToListAsync();
        roleIds.AddRange(platformRoleEntities);

        roleIds.AddRange(storeRoles.Select(sr => sr.RoleId));
        roleIds.AddRange(organizationRoles.Select(or => or.RoleId));
        roleIds.AddRange(storeCustomerRoles.Select(scr => scr.RoleId));

        var permissions = await context.Set<RolePermission>()
            .AsNoTracking()
            .Include(rp => rp.Permission)
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync();

        return new UserRolesAndPermissionsDto(
            PlatformRoles: platformRoles.ToList(),
            StoreRoles: storeRoles,
            OrganizationRoles: organizationRoles,
            StoreCustomerRoles: storeCustomerRoles,
            Permissions: permissions);
    }
}

