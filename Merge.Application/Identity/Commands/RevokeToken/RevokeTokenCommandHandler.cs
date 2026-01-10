using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Common;
using Merge.Domain.Entities;
using RefreshTokenEntity = Merge.Domain.Entities.RefreshToken;

namespace Merge.Application.Identity.Commands.RevokeToken;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RevokeTokenCommandHandler> _logger;

    public RevokeTokenCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<RevokeTokenCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Token revoke attempt. RefreshToken: {RefreshToken}", request.RefreshToken);

        // ✅ BOLUM 9.1: Refresh token hash'lenmiş olarak saklanıyor
        var tokenHash = TokenHasher.HashToken(request.RefreshToken);
        var refreshTokenEntity = await _context.Set<RefreshTokenEntity>()
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (refreshTokenEntity == null)
        {
            _logger.LogWarning("Token revoke failed - invalid token. RefreshToken: {RefreshToken}", request.RefreshToken);
            throw new BusinessException("Geçersiz refresh token.");
        }

        if (!refreshTokenEntity.IsActive)
        {
            _logger.LogInformation("Token already revoked. UserId: {UserId}", refreshTokenEntity.UserId);
            return Unit.Value;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        refreshTokenEntity.Revoke(request.IpAddress);

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Token revoked successfully. UserId: {UserId}",
            refreshTokenEntity.UserId);

        return Unit.Value;
    }
}

