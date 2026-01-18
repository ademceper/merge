using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Order.Commands.CreateOrderFromCart;
using Merge.Application.Cart.Commands.AddItemToCart;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Order.Commands.Reorder;

public class ReorderCommandHandler(IDbContext context, IMediator mediator, ILogger<ReorderCommandHandler> logger) : IRequestHandler<ReorderCommand, OrderDto>
{

    public async Task<OrderDto> Handle(ReorderCommand request, CancellationToken cancellationToken)
    {
        var originalOrder = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == request.UserId, cancellationToken);

        if (originalOrder is null)
        {
            throw new NotFoundException("Sipariş", request.OrderId);
        }

        var addedItems = 0;
        var skippedItems = 0;

        // Sepete ekle
        foreach (var orderItem in originalOrder.OrderItems)
        {
            if (orderItem.Product.IsActive && orderItem.Product.StockQuantity > 0)
            {
                try
                {
                    await mediator.Send(new AddItemToCartCommand(request.UserId, orderItem.ProductId, orderItem.Quantity), cancellationToken);
                    addedItems++;
                }
                catch (Exception ex)
                {
                    // Ancak reorder işleminde bir ürün eklenemezse diğer ürünler eklenmeye devam etmeli
                    // Bu durumda warning log'lanıp işlem devam ediyor (business requirement)
                    logger.LogWarning(ex, "Failed to add product to cart during reorder. ProductId: {ProductId}", orderItem.ProductId);
                    skippedItems++;
                }
            }
            else
            {
                skippedItems++;
            }
        }

        logger.LogInformation(
            "Reorder completed. OriginalOrderId: {OrderId}, AddedItems: {AddedItems}, SkippedItems: {SkippedItems}",
            request.OrderId, addedItems, skippedItems);

        return await mediator.Send(new CreateOrderFromCartCommand(request.UserId, originalOrder.AddressId), cancellationToken);
    }
}
