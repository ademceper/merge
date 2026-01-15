using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Order.Commands.CancelOrderSplit;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CancelOrderSplitCommandHandler : IRequestHandler<CancelOrderSplitCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelOrderSplitCommandHandler> _logger;

    public CancelOrderSplitCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<CancelOrderSplitCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(CancelOrderSplitCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var split = await _context.Set<OrderSplit>()
            .AsSplitQuery()
            .Include(s => s.SplitOrder)
            .Include(s => s.OriginalOrder)
            .FirstOrDefaultAsync(s => s.Id == request.SplitId, cancellationToken);

        if (split == null) return false;

        if (split.SplitOrder.Status != OrderStatus.Pending)
        {
            throw new BusinessException("Beklemede durumunda olmayan bölünmüş sipariş iptal edilemez.");
        }

        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes with ThenInclude)
        var splitItems = await _context.Set<OrderSplitItem>()
            .AsSplitQuery()
            .Include(si => si.OriginalOrderItem)
                .ThenInclude(oi => oi.Product)
            .Include(si => si.SplitOrderItem)
                .ThenInclude(oi => oi.Product)
            .Where(si => si.OrderSplitId == request.SplitId)
            .ToListAsync(cancellationToken);

        foreach (var splitItem in splitItems)
        {
            var originalItem = splitItem.OriginalOrderItem;
            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan (UpdateQuantity)
            var newQuantity = originalItem.Quantity + splitItem.Quantity;
            originalItem.UpdateQuantity(newQuantity);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        var originalOrder = split.OriginalOrder;
        originalOrder.RecalculateTotals();

        split.SplitOrder.MarkAsDeleted();
        split.MarkAsDeleted();

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        split.Cancel();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Order split cancelled. SplitId: {SplitId}", request.SplitId);
        
        return true;
    }
}
