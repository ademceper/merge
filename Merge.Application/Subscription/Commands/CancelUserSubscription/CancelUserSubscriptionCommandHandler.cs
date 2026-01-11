using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Subscription.Commands.CancelUserSubscription;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CancelUserSubscriptionCommandHandler : IRequestHandler<CancelUserSubscriptionCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelUserSubscriptionCommandHandler> _logger;

    public CancelUserSubscriptionCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<CancelUserSubscriptionCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(CancelUserSubscriptionCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Cancelling user subscription. SubscriptionId: {SubscriptionId}",
            request.SubscriptionId);

        // ✅ NOT: AsNoTracking() YOK - Entity track edilmeli (update için)
        var subscription = await _context.Set<UserSubscription>()
            .FirstOrDefaultAsync(us => us.Id == request.SubscriptionId, cancellationToken);

        if (subscription == null)
        {
            throw new NotFoundException("Abonelik", request.SubscriptionId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        subscription.Cancel(request.Reason);

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("User subscription cancelled successfully. SubscriptionId: {SubscriptionId}",
            subscription.Id);

        return true;
    }
}
