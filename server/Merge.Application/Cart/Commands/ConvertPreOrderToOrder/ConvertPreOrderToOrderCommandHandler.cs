using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using AddressEntity = Merge.Domain.Modules.Identity.Address;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.ConvertPreOrderToOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class ConvertPreOrderToOrderCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<ConvertPreOrderToOrderCommandHandler> logger) : IRequestHandler<ConvertPreOrderToOrderCommand, bool>
{

    public async Task<bool> Handle(ConvertPreOrderToOrderCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var preOrder = await context.Set<PreOrder>()
            .AsSplitQuery()
                .Include(po => po.Product)
                .Include(po => po.User)
                .FirstOrDefaultAsync(po => po.Id == request.PreOrderId, cancellationToken);

            // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
            if (preOrder is null) return false;

            if (preOrder.Status == PreOrderStatus.Converted)
            {
                throw new BusinessException("Ön sipariş zaten dönüştürülmüş.");
            }

            var address = await context.Set<AddressEntity>()
                .FirstOrDefaultAsync(a => a.UserId == preOrder.UserId && a.IsDefault, cancellationToken);

            // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
            if (address is null)
            {
                address = await context.Set<AddressEntity>()
                    .FirstOrDefaultAsync(a => a.UserId == preOrder.UserId, cancellationToken);
            }

            // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
            if (address is null)
            {
                throw new BusinessException("Sipariş oluşturmak için adres bilgisi gereklidir.");
            }

            var order = Merge.Domain.Modules.Ordering.Order.Create(preOrder.UserId, address.Id, address);

            var product = await context.Set<Merge.Domain.Modules.Catalog.Product>()
                .FirstOrDefaultAsync(p => p.Id == preOrder.ProductId, cancellationToken);

            // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
            if (product is null)
            {
                throw new NotFoundException("Ürün", preOrder.ProductId);
            }

            order.AddItem(product, preOrder.Quantity);

            var shippingCost = new Money(0);
            order.SetShippingCost(shippingCost);

            var tax = new Money(0);
            order.SetTax(tax);

            await context.Set<Merge.Domain.Modules.Ordering.Order>().AddAsync(order, cancellationToken);

            preOrder.ConvertToOrder(order.Id);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "PreOrder siparise donusturme hatasi. PreOrderId: {PreOrderId}",
                request.PreOrderId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

