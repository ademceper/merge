using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Common;
using static Merge.Application.Common.LogMasking;
using Merge.Domain.Entities;
using RefreshTokenEntity = Merge.Domain.Modules.Identity.RefreshToken;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Identity.Commands.RevokeToken;

public class RevokeTokenCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<RevokeTokenCommandHandler> logger) : IRequestHandler<RevokeTokenCommand, Unit>
{

    public async Task<Unit> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Token revoke attempt. RefreshToken: {RefreshToken}", LogMasking.MaskToken(request.RefreshToken));

        var tokenHash = TokenHasher.HashToken(request.RefreshToken);
        var refreshTokenEntity = await context.Set<RefreshTokenEntity>()
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (refreshTokenEntity is null)
        {
            logger.LogWarning("Token revoke failed - invalid token. RefreshToken: {RefreshToken}", LogMasking.MaskToken(request.RefreshToken));
            throw new BusinessException("Geçersiz refresh token.");
        }

        if (!refreshTokenEntity.IsActive)
        {
            logger.LogInformation("Token already revoked. UserId: {UserId}", refreshTokenEntity.UserId);
            return Unit.Value;
        }

        refreshTokenEntity.Revoke(request.IpAddress);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Token revoked successfully. UserId: {UserId}",
            refreshTokenEntity.UserId);

        return Unit.Value;
    }
}

