using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.B2B.Commands.ApprovePurchaseOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class ApprovePurchaseOrderCommandHandler : IRequestHandler<ApprovePurchaseOrderCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApprovePurchaseOrderCommandHandler> _logger;

    public ApprovePurchaseOrderCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ApprovePurchaseOrderCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ApprovePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Approving purchase order. PurchaseOrderId: {PurchaseOrderId}, ApprovedByUserId: {ApprovedByUserId}",
            request.Id, request.ApprovedByUserId);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (PurchaseOrder + CreditTerm updates)
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
            var po = await _context.Set<PurchaseOrder>()
                .FirstOrDefaultAsync(po => po.Id == request.Id, cancellationToken);

            if (po == null)
            {
                _logger.LogWarning("Purchase order not found with Id: {PurchaseOrderId}", request.Id);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return false;
            }

            // Check credit limit if credit term is used
            if (po.CreditTermId.HasValue)
            {
                // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
                var creditTerm = await _context.Set<CreditTerm>()
                    .FirstOrDefaultAsync(ct => ct.Id == po.CreditTermId.Value, cancellationToken);

                if (creditTerm != null && creditTerm.CreditLimit.HasValue)
                {
                    // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
                    // Entity method içinde zaten credit limit kontrolü var
                    creditTerm.UseCredit(po.TotalAmount);
                }
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
            // ✅ ARCHITECTURE: Domain event'ler entity içinde oluşturuluyor (PurchaseOrder.Approve() içinde PurchaseOrderApprovedEvent)
            po.Approve(request.ApprovedByUserId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Purchase order approved successfully. PurchaseOrderId: {PurchaseOrderId}", request.Id);
            return true;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "PurchaseOrder onaylama hatasi. PurchaseOrderId: {PurchaseOrderId}, ApprovedByUserId: {ApprovedByUserId}",
                request.Id, request.ApprovedByUserId);
            // ✅ ARCHITECTURE: Hata olursa ROLLBACK - hiçbir şey yazılmaz
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

