using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Subscription.Commands.ActivateSubscription;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ActivateSubscriptionCommandHandler : IRequestHandler<ActivateSubscriptionCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ActivateSubscriptionCommandHandler> _logger;

    public ActivateSubscriptionCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ActivateSubscriptionCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ActivateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Activating subscription. SubscriptionId: {SubscriptionId}", request.SubscriptionId);

        // ✅ NOT: AsNoTracking() YOK - Entity track edilmeli (update için)
        var subscription = await _context.Set<UserSubscription>()
            .FirstOrDefaultAsync(us => us.Id == request.SubscriptionId, cancellationToken);

        if (subscription == null)
        {
            throw new NotFoundException("Abonelik", request.SubscriptionId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        subscription.Activate();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Subscription activated successfully. SubscriptionId: {SubscriptionId}", subscription.Id);

        return true;
    }
}
