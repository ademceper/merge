using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Cart;
using Merge.Application.Exceptions;
using Merge.Application.Order.Commands.CreateOrderFromCart;
using OrderEntity = Merge.Domain.Entities.Order;

namespace Merge.Application.Order.Commands.Reorder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ReorderCommandHandler : IRequestHandler<ReorderCommand, OrderDto>
{
    private readonly IDbContext _context;
    private readonly IMediator _mediator;
    private readonly ICartService _cartService;
    private readonly ILogger<ReorderCommandHandler> _logger;

    public ReorderCommandHandler(
        IDbContext context,
        IMediator mediator,
        ICartService cartService,
        ILogger<ReorderCommandHandler> logger)
    {
        _context = context;
        _mediator = mediator;
        _cartService = cartService;
        _logger = logger;
    }

    public async Task<OrderDto> Handle(ReorderCommand request, CancellationToken cancellationToken)
    {
        var originalOrder = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == request.UserId, cancellationToken);

        if (originalOrder == null)
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
                    await _cartService.AddItemToCartAsync(request.UserId, orderItem.ProductId, orderItem.Quantity, cancellationToken);
                    addedItems++;
                }
                catch (Exception ex)
                {
                    // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
                    // Ancak reorder işleminde bir ürün eklenemezse diğer ürünler eklenmeye devam etmeli
                    // Bu durumda warning log'lanıp işlem devam ediyor (business requirement)
                    _logger.LogWarning(ex, "Failed to add product to cart during reorder. ProductId: {ProductId}", orderItem.ProductId);
                    skippedItems++;
                }
            }
            else
            {
                skippedItems++;
            }
        }

        _logger.LogInformation(
            "Reorder completed. OriginalOrderId: {OrderId}, AddedItems: {AddedItems}, SkippedItems: {SkippedItems}",
            request.OrderId, addedItems, skippedItems);

        // ✅ MediatR: Yeni sipariş oluşturmak için CreateOrderFromCartCommand kullan
        return await _mediator.Send(new CreateOrderFromCartCommand(request.UserId, originalOrder.AddressId), cancellationToken);
    }
}
