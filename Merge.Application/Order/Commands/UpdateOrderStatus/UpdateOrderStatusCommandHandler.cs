using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using OrderEntity = Merge.Domain.Entities.Order;

namespace Merge.Application.Order.Commands.UpdateOrderStatus;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, OrderDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;

    public UpdateOrderStatusCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateOrderStatusCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OrderDto> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Set<OrderEntity>()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException("Sipariş", request.OrderId);
        }

        var oldStatus = order.Status;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        order.TransitionTo(request.Status);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Single query with all includes
        order = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        _logger.LogInformation(
            "Order status updated. OrderId: {OrderId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
            request.OrderId, oldStatus, request.Status);

        return _mapper.Map<OrderDto>(order!);
    }
}
