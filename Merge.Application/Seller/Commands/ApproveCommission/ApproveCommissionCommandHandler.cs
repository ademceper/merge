using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.ApproveCommission;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ApproveCommissionCommandHandler : IRequestHandler<ApproveCommissionCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApproveCommissionCommandHandler> _logger;

    public ApproveCommissionCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ApproveCommissionCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ApproveCommissionCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Approving commission. CommissionId: {CommissionId}", request.CommissionId);

        // ✅ PERFORMANCE: Removed manual !sc.IsDeleted (Global Query Filter)
        var commission = await _context.Set<SellerCommission>()
            .FirstOrDefaultAsync(sc => sc.Id == request.CommissionId, cancellationToken);

        if (commission == null)
        {
            _logger.LogWarning("Commission not found. CommissionId: {CommissionId}", request.CommissionId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        commission.Approve();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Commission approved. CommissionId: {CommissionId}", request.CommissionId);

        return true;
    }
}
