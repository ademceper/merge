using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Enums;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Order.Commands.CancelOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelOrderCommandHandler> _logger;

    public CancelOrderCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<CancelOrderCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Set<OrderEntity>()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            return false;
        }

        if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Shipped)
        {
            throw new BusinessException("Bu sipariş iptal edilemez.");
        }

        // ✅ CRITICAL: Transaction for atomic stock restoration
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            foreach (var item in order.OrderItems)
            {
                item.Product.IncreaseStock(item.Quantity);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            order.Cancel();

            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation(
                "Order cancelled successfully. OrderId: {OrderId}, ItemsRestored: {ItemCount}",
                request.OrderId, order.OrderItems.Count);

            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Order cancellation failed. OrderId: {OrderId}", request.OrderId);
            throw;
        }
    }
}
