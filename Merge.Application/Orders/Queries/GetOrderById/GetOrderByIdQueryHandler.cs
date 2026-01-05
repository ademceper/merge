using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces.Order;
using Merge.Application.Exceptions;

namespace Merge.Application.Orders.Queries.GetOrderById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderService _orderService;
    private readonly ILogger<GetOrderByIdQueryHandler> _logger;

    public GetOrderByIdQueryHandler(
        IOrderService orderService,
        ILogger<GetOrderByIdQueryHandler> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting order {OrderId}", request.OrderId);

        var order = await _orderService.GetByIdAsync(request.OrderId, cancellationToken);
        
        return order;
    }
}

