using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Subscription.Commands.DeleteSubscriptionPlan;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteSubscriptionPlanCommandHandler : IRequestHandler<DeleteSubscriptionPlanCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteSubscriptionPlanCommandHandler> _logger;

    public DeleteSubscriptionPlanCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteSubscriptionPlanCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Deleting subscription plan. PlanId: {PlanId}", request.Id);

        // ✅ NOT: AsNoTracking() YOK - Entity track edilmeli (delete için)
        var plan = await _context.Set<SubscriptionPlan>()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (plan == null)
        {
            throw new NotFoundException("Abonelik planı", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        plan.Delete();
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Subscription plan deleted successfully. PlanId: {PlanId}", plan.Id);

        return true;
    }
}
