using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Application.Services.Notification;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.NotifyPreOrderAvailable;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class NotifyPreOrderAvailableCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IEmailService emailService) : IRequestHandler<NotifyPreOrderAvailableCommand>
{

    public async Task Handle(NotifyPreOrderAvailableCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var preOrder = await context.Set<Merge.Domain.Modules.Ordering.PreOrder>()
            .AsSplitQuery()
            .Include(po => po.Product)
            .Include(po => po.User)
            .FirstOrDefaultAsync(po => po.Id == request.PreOrderId, cancellationToken);

        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (preOrder is null) return;

        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (preOrder.NotificationSentAt is not null) return;

        await emailService.SendEmailAsync(
            preOrder.User.Email ?? string.Empty,
            "Your Pre-Order is Ready!",
            $"Good news! Your pre-order for {preOrder.Product.Name} is now available and ready to ship.",
            false,
            cancellationToken
        );

        preOrder.MarkNotificationAsSent();
        preOrder.MarkAsReadyToShip();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

