using MediatR;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Merge.Application.Configuration;
using Merge.Application.Exceptions;

namespace Merge.Application.Identity.Queries.ValidateToken;

public class ValidateTokenQueryHandler(
    IOptions<JwtSettings> jwtSettings,
    ILogger<ValidateTokenQueryHandler> logger) : IRequestHandler<ValidateTokenQuery, bool>
{

    public Task<bool> Handle(ValidateTokenQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Token validation attempt");

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtSettings.Value.Key ?? throw new ConfigurationException("JWT Key bulunamadı"));

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

            logger.LogInformation("Token validation successful");
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Token validation failed. Error: {ErrorType}", ex.GetType().Name);
            return Task.FromResult(false);
        }
    }
}

