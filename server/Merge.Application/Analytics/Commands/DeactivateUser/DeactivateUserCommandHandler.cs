using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Commands.DeactivateUser;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeactivateUserCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeactivateUserCommandHandler> logger) : IRequestHandler<DeactivateUserCommand, bool>
{

    public async Task<bool> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deactivating user. UserId: {UserId}", request.UserId);
        
        // ✅ FIX: Use FirstOrDefaultAsync instead of FindAsync to respect Global Query Filter
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            logger.LogWarning("User not found for deactivation. UserId: {UserId}", request.UserId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        user.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("User deactivated successfully. UserId: {UserId}", request.UserId);
        return true;
    }
}

