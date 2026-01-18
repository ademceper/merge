using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using UserEntity = Merge.Domain.Modules.Identity.User;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Order.Commands.CreateReturnRequest;

public class CreateReturnRequestCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateReturnRequestCommandHandler> logger,
    IOptions<OrderSettings> orderSettings) : IRequestHandler<CreateReturnRequestCommand, ReturnRequestDto>
{
    private readonly OrderSettings _orderSettings = orderSettings.Value;

    public async Task<ReturnRequestDto> Handle(CreateReturnRequestCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Return request oluşturuluyor. OrderId: {OrderId}, UserId: {UserId}, Reason: {Reason}",
            request.Dto.OrderId, request.Dto.UserId, request.Dto.Reason);

        if (request.Dto == null)
        {
            throw new ArgumentNullException(nameof(request.Dto));
        }

        if (request.Dto.OrderItemIds == null || request.Dto.OrderItemIds.Count == 0)
        {
            throw new ValidationException("İade edilecek ürün seçilmedi.");
        }

        if (string.IsNullOrWhiteSpace(request.Dto.Reason))
        {
            throw new ValidationException("İade nedeni boş olamaz.");
        }

        var order = await context.Set<OrderEntity>()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == request.Dto.OrderId && o.UserId == request.Dto.UserId, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException("Sipariş", request.Dto.OrderId);
        }

        if (order.Status != OrderStatus.Delivered)
        {
            throw new BusinessException("Sadece teslim edilmiş siparişler için iade yapılabilir.");
        }

        if (order.DeliveredDate.HasValue && (DateTime.UtcNow - order.DeliveredDate.Value).TotalDays > _orderSettings.ReturnPeriodDays)
        {
            throw new BusinessException($"İade süresi dolmuş. Teslim tarihinden itibaren {_orderSettings.ReturnPeriodDays} gün içinde iade yapılabilir.");
        }

        var refundAmount = await context.Set<OrderItem>()
            .Where(oi => oi.OrderId == request.Dto.OrderId && request.Dto.OrderItemIds.Contains(oi.Id))
            .SumAsync(oi => (decimal?)oi.TotalPrice, cancellationToken) ?? 0;

        if (refundAmount == 0)
        {
            throw new ValidationException("İade edilecek ürün seçilmedi.");
        }

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Dto.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", request.Dto.UserId);
        }

        var refundAmountMoney = new Money(refundAmount);
        var returnRequest = ReturnRequest.Create(
            request.Dto.OrderId,
            request.Dto.UserId,
            request.Dto.Reason,
            refundAmountMoney,
            request.Dto.OrderItemIds,
            order,
            user);

        await context.Set<ReturnRequest>().AddAsync(returnRequest, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var reloadedReturnRequest = await context.Set<ReturnRequest>()
            .AsNoTracking()
            .Include(r => r.Order)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == returnRequest.Id, cancellationToken);

        if (reloadedReturnRequest == null)
        {
            logger.LogWarning("Return request {ReturnRequestId} not found after creation", returnRequest.Id);
            return mapper.Map<ReturnRequestDto>(returnRequest);
        }

        logger.LogInformation(
            "Return request oluşturuldu. ReturnRequestId: {ReturnRequestId}, OrderId: {OrderId}, RefundAmount: {RefundAmount}",
            reloadedReturnRequest.Id, request.Dto.OrderId, refundAmount);

        return mapper.Map<ReturnRequestDto>(reloadedReturnRequest);
    }
}
