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
public class NotifyPreOrderAvailableCommandHandler : IRequestHandler<NotifyPreOrderAvailableCommand>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public NotifyPreOrderAvailableCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IEmailService emailService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task Handle(NotifyPreOrderAvailableCommand request, CancellationToken cancellationToken)
    {
        var preOrder = await _context.Set<Merge.Domain.Modules.Ordering.PreOrder>()
            .Include(po => po.Product)
            .Include(po => po.User)
            .FirstOrDefaultAsync(po => po.Id == request.PreOrderId, cancellationToken);

        if (preOrder == null) return;

        if (preOrder.NotificationSentAt != null) return;

        await _emailService.SendEmailAsync(
            preOrder.User.Email ?? string.Empty,
            "Your Pre-Order is Ready!",
            $"Good news! Your pre-order for {preOrder.Product.Name} is now available and ready to ship."
        );

        preOrder.MarkNotificationAsSent();
        preOrder.MarkAsReadyToShip();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

