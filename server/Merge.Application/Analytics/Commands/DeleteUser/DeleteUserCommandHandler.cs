using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Commands.DeleteUser;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeleteUserCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteUserCommandHandler> logger) : IRequestHandler<DeleteUserCommand, bool>
{

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting user. UserId: {UserId}", request.UserId);
        
        // ✅ FIX: Use FirstOrDefaultAsync instead of FindAsync to respect Global Query Filter
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            logger.LogWarning("User not found for deletion. UserId: {UserId}", request.UserId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Soft delete
        user.MarkAsDeleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("User deleted successfully. UserId: {UserId}", request.UserId);
        return true;
    }
}

