using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using Merge.Domain.SharedKernel.DomainEvents;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.SubmitPurchaseOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class SubmitPurchaseOrderCommandHandler : IRequestHandler<SubmitPurchaseOrderCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SubmitPurchaseOrderCommandHandler> _logger;

    public SubmitPurchaseOrderCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<SubmitPurchaseOrderCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(SubmitPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Submitting purchase order. PurchaseOrderId: {PurchaseOrderId}", request.Id);

        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var po = await _context.Set<PurchaseOrder>()
            .FirstOrDefaultAsync(po => po.Id == request.Id, cancellationToken);

        if (po == null)
        {
            _logger.LogWarning("Purchase order not found with Id: {PurchaseOrderId}", request.Id);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
        // ✅ ARCHITECTURE: Domain event'ler entity içinde oluşturuluyor (PurchaseOrder.Submit() içinde PurchaseOrderSubmittedEvent)
        po.Submit();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Purchase order submitted successfully. PurchaseOrderId: {PurchaseOrderId}", request.Id);
        return true;
    }
}

