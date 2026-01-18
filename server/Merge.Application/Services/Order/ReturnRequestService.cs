using AutoMapper;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Order;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Application.DTOs.Order;
using Merge.Application.Common;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using Merge.Domain.ValueObjects;
using ReturnRequest = Merge.Domain.Modules.Ordering.ReturnRequest;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IReturnRequestRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Ordering.ReturnRequest>;
using IOrderRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Ordering.Order>;

namespace Merge.Application.Services.Order;

public class ReturnRequestService(IReturnRequestRepository returnRequestRepository, IOrderRepository orderRepository, IDbContext context, IMapper mapper, IUnitOfWork unitOfWork, ILogger<ReturnRequestService> logger) : IReturnRequestService
{

    public async Task<ReturnRequestDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var returnRequest = await context.Set<ReturnRequest>()
            .AsNoTracking()
            .Include(r => r.Order)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (returnRequest is null) return null;

        // Not: OrderNumber ve UserName AutoMapper'da zaten map ediliyor
        return mapper.Map<ReturnRequestDto>(returnRequest);
    }

    public async Task<PagedResult<ReturnRequestDto>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100; // Max limit
        if (page < 1) page = 1;

        var query = context.Set<ReturnRequest>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(r => r.Order)
            .Include(r => r.User)
            .Where(r => r.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var returnRequests = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Not: OrderNumber ve UserName AutoMapper'da zaten map ediliyor
        var dtos = mapper.Map<IEnumerable<ReturnRequestDto>>(returnRequests);

        return new PagedResult<ReturnRequestDto>
        {
            Items = dtos.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<ReturnRequestDto>> GetAllAsync(string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100; // Max limit
        if (page < 1) page = 1;

        IQueryable<ReturnRequest> query = context.Set<ReturnRequest>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(r => r.Order)
            .Include(r => r.User);

        if (!string.IsNullOrEmpty(status))
        {
            var statusEnum = Enum.Parse<ReturnRequestStatus>(status);
            query = query.Where(r => r.Status == statusEnum);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var returnRequests = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Not: OrderNumber ve UserName AutoMapper'da zaten map ediliyor
        var dtos = mapper.Map<IEnumerable<ReturnRequestDto>>(returnRequests);

        return new PagedResult<ReturnRequestDto>
        {
            Items = dtos.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ReturnRequestDto> CreateAsync(CreateReturnRequestDto dto, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Return request oluşturuluyor. OrderId: {OrderId}, UserId: {UserId}, Reason: {Reason}",
            dto.OrderId, dto.UserId, dto.Reason);

        ArgumentNullException.ThrowIfNull(dto);

        if (dto.OrderItemIds is null || !dto.OrderItemIds.Any())
        {
            throw new ValidationException("İade edilecek ürün seçilmedi.");
        }

        if (string.IsNullOrWhiteSpace(dto.Reason))
        {
            throw new ValidationException("İade nedeni boş olamaz.");
        }

        var order = await context.Set<OrderEntity>()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.UserId == dto.UserId, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException("Sipariş", dto.OrderId);
        }

        if (order.Status != OrderStatus.Delivered)
        {
            throw new BusinessException("Sadece teslim edilmiş siparişler için iade yapılabilir.");
        }

        // Teslim edildikten sonra 14 gün içinde iade yapılabilir
        if (order.DeliveredDate.HasValue && (DateTime.UtcNow - order.DeliveredDate.Value).TotalDays > 14)
        {
            throw new BusinessException("İade süresi dolmuş. Teslim tarihinden itibaren 14 gün içinde iade yapılabilir.");
        }

        // İade edilecek kalemlerin toplam tutarını hesapla
        var refundAmount = await context.Set<OrderItem>()
            .Where(oi => oi.OrderId == dto.OrderId && dto.OrderItemIds.Contains(oi.Id))
            .SumAsync(oi => (decimal?)oi.TotalPrice, cancellationToken) ?? 0;

        if (refundAmount == 0)
        {
            throw new ValidationException("İade edilecek ürün seçilmedi.");
        }

        // Order zaten yukarıda query edildi, tekrar query etme
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == dto.UserId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("Kullanıcı", dto.UserId);
        }

        var refundAmountMoney = new Money(refundAmount);
        var returnRequest = ReturnRequest.Create(
            dto.OrderId,
            dto.UserId,
            dto.Reason,
            refundAmountMoney,
            dto.OrderItemIds.ToList(),
            order,
            user);

        returnRequest = await returnRequestRepository.AddAsync(returnRequest);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var reloadedReturnRequest = await context.Set<ReturnRequest>()
            .AsNoTracking()
            .Include(r => r.Order)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == returnRequest.Id, cancellationToken);

        if (reloadedReturnRequest is null)
        {
            logger.LogWarning("Return request {ReturnRequestId} not found after creation", returnRequest.Id);
            return mapper.Map<ReturnRequestDto>(returnRequest);
        }

        logger.LogInformation(
            "Return request oluşturuldu. ReturnRequestId: {ReturnRequestId}, OrderId: {OrderId}, RefundAmount: {RefundAmount}",
            reloadedReturnRequest.Id, dto.OrderId, refundAmount);

        // Not: OrderNumber ve UserName AutoMapper'da zaten map ediliyor
        return mapper.Map<ReturnRequestDto>(reloadedReturnRequest);
    }

    public async Task<ReturnRequestDto> UpdateStatusAsync(Guid id, string status, string? rejectionReason = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ValidationException("Durum boş olamaz.");
        }

        var returnRequest = await returnRequestRepository.GetByIdAsync(id);
        if (returnRequest is null)
        {
            throw new NotFoundException("İade talebi", id);
        }

        var parsedStatus = Enum.Parse<ReturnRequestStatus>(status);
        if (parsedStatus == ReturnRequestStatus.Rejected && !string.IsNullOrEmpty(rejectionReason))
        {
            returnRequest.Reject(rejectionReason);
        }
        else if (parsedStatus == ReturnRequestStatus.Approved)
        {
            returnRequest.Approve();
        }
        else if (parsedStatus == ReturnRequestStatus.Completed)
        {
            returnRequest.Complete();
        }

        await returnRequestRepository.UpdateAsync(returnRequest);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        returnRequest = await context.Set<ReturnRequest>()
            .AsNoTracking()
            .Include(r => r.Order)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        // Not: OrderNumber ve UserName AutoMapper'da zaten map ediliyor
        return mapper.Map<ReturnRequestDto>(returnRequest);
    }

    public async Task<bool> ApproveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var returnRequest = await returnRequestRepository.GetByIdAsync(id);
        if (returnRequest is null)
        {
            return false;
        }

        returnRequest.Approve();
        await returnRequestRepository.UpdateAsync(returnRequest);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Ödeme iadesi yapılacak (Payment service ile entegre edilecek)
        // Burada sadece status güncelleniyor

        return true;
    }

    public async Task<bool> RejectAsync(Guid id, string reason, CancellationToken cancellationToken = default)
    {
        return await UpdateStatusAsync(id, "Rejected", reason, cancellationToken) is not null;
    }

    public async Task<bool> CompleteAsync(Guid id, string trackingNumber, CancellationToken cancellationToken = default)
    {
        var returnRequest = await returnRequestRepository.GetByIdAsync(id);
        if (returnRequest is null)
        {
            return false;
        }

        returnRequest.Complete(trackingNumber);
        await returnRequestRepository.UpdateAsync(returnRequest);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

