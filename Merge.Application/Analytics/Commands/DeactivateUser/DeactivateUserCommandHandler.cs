using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;

namespace Merge.Application.Analytics.Commands.DeactivateUser;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeactivateUserCommandHandler> _logger;

    public DeactivateUserCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeactivateUserCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating user. UserId: {UserId}", request.UserId);
        
        // ✅ FIX: Use FirstOrDefaultAsync instead of FindAsync to respect Global Query Filter
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User not found for deactivation. UserId: {UserId}", request.UserId);
            return false;
        }

        user.EmailConfirmed = false;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("User deactivated successfully. UserId: {UserId}", request.UserId);
        return true;
    }
}

