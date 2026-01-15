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
        logger.LogInformation("Registration attempt. Email: {Email}", request.Email);

        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            logger.LogWarning("Registration failed - email already exists. Email: {Email}", request.Email);
            throw new BusinessException("Bu email adresi zaten kullanılıyor.");
        }

        var user = UserEntity.Create(request.FirstName, request.LastName, request.Email, request.PhoneNumber);

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            logger.LogWarning("Registration failed - validation errors. Email: {Email}, Errors: {Errors}", request.Email, errors);
            throw new ValidationException("Kullanıcı oluşturulamadı.", errors);
        }

        await userManager.AddToRoleAsync(user, securitySettings.Value.DefaultUserRole);

        var roles = await userManager.GetRolesAsync(user);

        var userEntry = context.Users.Attach(user);
        userEntry.State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;

        logger.LogInformation(
            "User registered successfully. UserId: {UserId}, Email: {Email}",
            user.Id, request.Email);

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

