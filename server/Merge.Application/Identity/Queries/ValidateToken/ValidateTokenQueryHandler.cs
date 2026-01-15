using MediatR;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Merge.Application.Configuration;

namespace Merge.Application.Identity.Queries.ValidateToken;

public class ValidateTokenQueryHandler(
    IOptions<JwtSettings> jwtSettings,
    ILogger<ValidateTokenQueryHandler> logger) : IRequestHandler<ValidateTokenQuery, bool>
{

    public Task<bool> Handle(ValidateTokenQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Token validation attempt. Token: {Token}", request.Token);

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtSettings.Value.Key ?? throw new InvalidOperationException("JWT Key bulunamadı"));

            tokenHandler.ValidateToken(request.Token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Value.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Value.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(jwtSettings.Value.ClockSkewSeconds)
            }, out _);

            logger.LogInformation("Token validation successful. Token: {Token}", request.Token);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Token validation failed. Token: {Token}", request.Token);
            return Task.FromResult(false);
        }
    }
}

