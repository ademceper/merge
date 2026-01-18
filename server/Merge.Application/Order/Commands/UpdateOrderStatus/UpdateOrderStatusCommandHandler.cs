using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Modules.Ordering;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Order.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<UpdateOrderStatusCommandHandler> logger) : IRequestHandler<UpdateOrderStatusCommand, bool>
{

    public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating order status. OrderId: {OrderId}, NewStatus: {NewStatus}",
            request.OrderId, request.Status);

        var order = await context.Set<OrderEntity>()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException("Sipariş", request.OrderId);
        }

        try
        {
            // TransitionTo methodu içinde status validasyonu ve Domain Event tetiklenmesi yapılır
            order.TransitionTo(request.Status);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Order status updated successfully. OrderId: {OrderId}, Status: {NewStatus}",
                request.OrderId, request.Status);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Order status update failed. OrderId: {OrderId}, NewStatus: {NewStatus}",
                request.OrderId, request.Status);
            throw;
        }
    }
}
