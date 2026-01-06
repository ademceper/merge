using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;

namespace Merge.Application.Analytics.Commands.ActivateUser;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ActivateUserCommandHandler> _logger;

    public ActivateUserCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ActivateUserCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Activating user. UserId: {UserId}", request.UserId);
        
        // ✅ FIX: Use FirstOrDefaultAsync instead of FindAsync to respect Global Query Filter
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User not found for activation. UserId: {UserId}", request.UserId);
            return false;
        }

        user.EmailConfirmed = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("User activated successfully. UserId: {UserId}", request.UserId);
        return true;
    }
}

