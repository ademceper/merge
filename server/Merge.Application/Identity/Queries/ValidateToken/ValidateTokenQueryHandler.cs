using MediatR;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Merge.Application.Configuration;

namespace Merge.Application.Identity.Queries.ValidateToken;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 12.1: Magic Number Sorunu - Configuration kullanımı
public class ValidateTokenQueryHandler : IRequestHandler<ValidateTokenQuery, bool>
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<ValidateTokenQueryHandler> _logger;

    public ValidateTokenQueryHandler(
        IOptions<JwtSettings> jwtSettings,
        ILogger<ValidateTokenQueryHandler> logger)
    {
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public Task<bool> Handle(ValidateTokenQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Token validation attempt. Token: {Token}", request.Token);

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key ?? throw new InvalidOperationException("JWT Key bulunamadı"));

            tokenHandler.ValidateToken(request.Token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(_jwtSettings.ClockSkewSeconds)
            }, out _);

            _logger.LogInformation("Token validation successful. Token: {Token}", request.Token);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed. Token: {Token}", request.Token);
            return Task.FromResult(false);
        }
    }
}

