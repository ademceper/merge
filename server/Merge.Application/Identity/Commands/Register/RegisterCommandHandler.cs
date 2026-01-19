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
using Microsoft.EntityFrameworkCore;
using Merge.Application.DTOs.Auth;
using Merge.Application.DTOs.User;
using Merge.Application.DTOs.Identity;
using Merge.Application.Exceptions;
using Merge.Application.Common;
using Merge.Application.Configuration;
using static Merge.Application.Common.LogMasking;
using UserEntity = Merge.Domain.Modules.Identity.User;
using RefreshTokenEntity = Merge.Domain.Modules.Identity.RefreshToken;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using Merge.Domain.SharedKernel.DomainEvents;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Identity.Commands.Register;

public class RegisterCommandHandler(
    UserManager<UserEntity> userManager,
    IDbContext context,
    IUnitOfWork unitOfWork,
    IOptions<JwtSettings> jwtSettings,
    IOptions<SecuritySettings> securitySettings,
    IMapper mapper,
    ILogger<RegisterCommandHandler> logger) : IRequestHandler<RegisterCommand, AuthResponseDto>
{

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Registration attempt. Email: {MaskedEmail}", MaskEmail(request.Email));

        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            logger.LogWarning("Registration failed - email already exists. Email: {MaskedEmail}", MaskEmail(request.Email));
            throw new BusinessException("Bu email adresi zaten kullanılıyor.");
        }

        var user = UserEntity.Create(request.FirstName, request.LastName, request.Email, request.PhoneNumber);

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            logger.LogWarning("Registration failed - validation errors. Email: {MaskedEmail}, Errors: {Errors}", MaskEmail(request.Email), errors);
            throw new ValidationException("Kullanıcı oluşturulamadı.", errors);
        }

        await userManager.AddToRoleAsync(user, securitySettings.Value.DefaultUserRole);

        var roles = await userManager.GetRolesAsync(user);

        var userEntry = context.Users.Attach(user);
        userEntry.State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;

        logger.LogInformation(
            "User registered successfully. UserId: {UserId}, Email: {MaskedEmail}",
            user.Id, MaskEmail(request.Email));

        return await GenerateAuthResponseAsync(user, roles, request.IpAddress, cancellationToken);
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(UserEntity user, IList<string> roles, string? ipAddress, CancellationToken cancellationToken)
    {
        var accessToken = await GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.Value.AccessTokenExpirationMinutes);

        var (refreshToken, plainToken) = GenerateRefreshToken(user.Id, ipAddress);
        
        user.AddDomainEvent(new UserRegisteredEvent(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            ipAddress));
        
        context.Set<RefreshTokenEntity>().Add(refreshToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var userDto = mapper.Map<UserDto>(user);
        userDto = userDto with { Role = roles.Count > 0 ? roles[0] : securitySettings.Value.DefaultUserRole };

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

        // Store roles (format: "store:{storeId}:{roleName}")
        foreach (var storeRole in rolesAndPermissions.StoreRoles)
        {
            claims.Add(new Claim("store_role", $"{storeRole.StoreId}:{storeRole.RoleName}"));
        }

        // Organization roles (format: "org:{orgId}:{roleName}")
        foreach (var orgRole in rolesAndPermissions.OrganizationRoles)
        {
            claims.Add(new Claim("org_role", $"{orgRole.OrganizationId}:{orgRole.RoleName}"));
        }

        // Store customer roles (format: "store_customer:{storeId}:{roleName}")
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
        // Platform roles
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return new UserRolesAndPermissionsDto([], [], [], [], []);
        }

        var platformRoles = await userManager.GetRolesAsync(user);

        // Store roles
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

        // Organization roles
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

        // Store customer roles
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

        // Get all permissions from all roles
        var roleIds = new List<Guid>();

        // Platform role IDs
        var platformRoleEntities = await context.Roles
            .AsNoTracking()
            .Where(r => platformRoles.Contains(r.Name ?? string.Empty))
            .Select(r => r.Id)
            .ToListAsync();
        roleIds.AddRange(platformRoleEntities);

        // Store role IDs
        roleIds.AddRange(storeRoles.Select(sr => sr.RoleId));

        // Organization role IDs
        roleIds.AddRange(organizationRoles.Select(or => or.RoleId));

        // Store customer role IDs
        roleIds.AddRange(storeCustomerRoles.Select(scr => scr.RoleId));

        // Get unique permissions
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

