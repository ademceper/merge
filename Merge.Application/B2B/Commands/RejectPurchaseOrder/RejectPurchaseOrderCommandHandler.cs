using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.B2B.Commands.RejectPurchaseOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class RejectPurchaseOrderCommandHandler : IRequestHandler<RejectPurchaseOrderCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RejectPurchaseOrderCommandHandler> _logger;

    public RejectPurchaseOrderCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<RejectPurchaseOrderCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(RejectPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Rejecting purchase order. PurchaseOrderId: {PurchaseOrderId}, Reason: {Reason}",
            request.Id, request.Reason);

        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var po = await _context.Set<PurchaseOrder>()
            .FirstOrDefaultAsync(po => po.Id == request.Id, cancellationToken);

        if (po == null)
        {
            _logger.LogWarning("Purchase order not found with Id: {PurchaseOrderId}", request.Id);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
        // ✅ ARCHITECTURE: Domain event'ler entity içinde oluşturuluyor (PurchaseOrder.Reject() içinde PurchaseOrderRejectedEvent)
        po.Reject(request.Reason);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Purchase order rejected successfully. PurchaseOrderId: {PurchaseOrderId}", request.Id);
        return true;
    }
}

