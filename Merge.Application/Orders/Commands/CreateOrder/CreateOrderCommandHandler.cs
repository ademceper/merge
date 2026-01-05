using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Order;
using Merge.Domain.Interfaces;
using Merge.Application.Interfaces.Cart;
using Merge.Application.Interfaces.Order;
using Merge.Application.Interfaces.Payment;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Orders.Commands.CreateOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderService _orderService;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IOrderService orderService,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating order for user {UserId}", request.UserId);

        if (!request.AddressId.HasValue)
        {
            throw new BusinessException("Shipping address is required");
        }

        var order = await _orderService.CreateOrderFromCartAsync(
            request.UserId, 
            request.AddressId.Value, 
            request.CouponCode, 
            cancellationToken);
        
        return order;
    }
}

