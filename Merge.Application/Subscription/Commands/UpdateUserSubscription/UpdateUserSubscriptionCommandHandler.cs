using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Subscription.Commands.UpdateUserSubscription;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UpdateUserSubscriptionCommandHandler : IRequestHandler<UpdateUserSubscriptionCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateUserSubscriptionCommandHandler> _logger;

    public UpdateUserSubscriptionCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UpdateUserSubscriptionCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateUserSubscriptionCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Updating user subscription. SubscriptionId: {SubscriptionId}", request.Id);

        // ✅ NOT: AsNoTracking() YOK - Entity track edilmeli (update için)
        var subscription = await _context.Set<UserSubscription>()
            .FirstOrDefaultAsync(us => us.Id == request.Id, cancellationToken);

        if (subscription == null)
        {
            throw new NotFoundException("Abonelik", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (request.AutoRenew.HasValue)
        {
            subscription.UpdateAutoRenew(request.AutoRenew.Value);
        }

        if (!string.IsNullOrEmpty(request.PaymentMethodId))
        {
            subscription.UpdatePaymentMethod(request.PaymentMethodId);
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("User subscription updated successfully. SubscriptionId: {SubscriptionId}", subscription.Id);

        return true;
    }
}
