using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateReturnRequestCommandHandler : IRequestHandler<CreateReturnRequestCommand, ReturnRequestDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateReturnRequestCommandHandler> _logger;
    private readonly OrderSettings _orderSettings;

    public CreateReturnRequestCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateReturnRequestCommandHandler> logger,
        IOptions<OrderSettings> orderSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _orderSettings = orderSettings.Value;
    }

    public async Task<ReturnRequestDto> Handle(CreateReturnRequestCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Return request oluşturuluyor. OrderId: {OrderId}, UserId: {UserId}, Reason: {Reason}",
            request.Dto.OrderId, request.Dto.UserId, request.Dto.Reason);

        if (request.Dto == null)
        {
            throw new ArgumentNullException(nameof(request.Dto));
        }

        // ✅ PERFORMANCE: Memory'de kontrol (DTO'dan geldiği için database query gerekmez)
        if (request.Dto.OrderItemIds == null || request.Dto.OrderItemIds.Count == 0)
        {
            throw new ValidationException("İade edilecek ürün seçilmedi.");
        }

        if (string.IsNullOrWhiteSpace(request.Dto.Reason))
        {
            throw new ValidationException("İade nedeni boş olamaz.");
        }

        var order = await _context.Set<OrderEntity>()
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

        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan
        if (order.DeliveredDate.HasValue && (DateTime.UtcNow - order.DeliveredDate.Value).TotalDays > _orderSettings.ReturnPeriodDays)
        {
            throw new BusinessException($"İade süresi dolmuş. Teslim tarihinden itibaren {_orderSettings.ReturnPeriodDays} gün içinde iade yapılabilir.");
        }

        var refundAmount = await _context.Set<OrderItem>()
            .Where(oi => oi.OrderId == request.Dto.OrderId && request.Dto.OrderItemIds.Contains(oi.Id))
            .SumAsync(oi => (decimal?)oi.TotalPrice, cancellationToken) ?? 0;

        if (refundAmount == 0)
        {
            throw new ValidationException("İade edilecek ürün seçilmedi.");
        }

        // ✅ PERFORMANCE: Order zaten yukarıda query edildi, tekrar query etme
        // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Dto.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", request.Dto.UserId);
        }

        // ✅ BOLUM 1.3: Value Objects - Money value object kullanımı
        var refundAmountMoney = new Merge.Domain.ValueObjects.Money(refundAmount);
        var returnRequest = ReturnRequest.Create(
            request.Dto.OrderId,
            request.Dto.UserId,
            request.Dto.Reason,
            refundAmountMoney,
            request.Dto.OrderItemIds,
            order,
            user);

        await _context.Set<ReturnRequest>().AddAsync(returnRequest, cancellationToken);
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var reloadedReturnRequest = await _context.Set<ReturnRequest>()
            .AsNoTracking()
            .Include(r => r.Order)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == returnRequest.Id, cancellationToken);

        if (reloadedReturnRequest == null)
        {
            _logger.LogWarning("Return request {ReturnRequestId} not found after creation", returnRequest.Id);
            return _mapper.Map<ReturnRequestDto>(returnRequest);
        }

        _logger.LogInformation(
            "Return request oluşturuldu. ReturnRequestId: {ReturnRequestId}, OrderId: {OrderId}, RefundAmount: {RefundAmount}",
            reloadedReturnRequest.Id, request.Dto.OrderId, refundAmount);

        return _mapper.Map<ReturnRequestDto>(reloadedReturnRequest);
    }
}
