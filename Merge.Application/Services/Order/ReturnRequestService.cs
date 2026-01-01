using AutoMapper;
using OrderEntity = Merge.Domain.Entities.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Order;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Order;


namespace Merge.Application.Services.Order;

public class ReturnRequestService : IReturnRequestService
{
    private readonly IRepository<ReturnRequest> _returnRequestRepository;
    private readonly IRepository<OrderEntity> _orderRepository;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReturnRequestService> _logger;

    public ReturnRequestService(
        IRepository<ReturnRequest> returnRequestRepository,
        IRepository<OrderEntity> orderRepository,
        ApplicationDbContext context,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ILogger<ReturnRequestService> logger)
    {
        _returnRequestRepository = returnRequestRepository;
        _orderRepository = orderRepository;
        _context = context;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ReturnRequestDto?> GetByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var returnRequest = await _context.ReturnRequests
            .AsNoTracking()
            .Include(r => r.Order)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (returnRequest == null) return null;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: OrderNumber ve UserName AutoMapper'da zaten map ediliyor
        return _mapper.Map<ReturnRequestDto>(returnRequest);
    }

    public async Task<IEnumerable<ReturnRequestDto>> GetByUserIdAsync(Guid userId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var returnRequests = await _context.ReturnRequests
            .AsNoTracking()
            .Include(r => r.Order)
            .Include(r => r.User)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper kullan
        // Not: OrderNumber ve UserName AutoMapper'da zaten map ediliyor
        return _mapper.Map<IEnumerable<ReturnRequestDto>>(returnRequests);
    }

    public async Task<IEnumerable<ReturnRequestDto>> GetAllAsync(string? status = null)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        IQueryable<ReturnRequest> query = _context.ReturnRequests
            .AsNoTracking()
            .Include(r => r.Order)
            .Include(r => r.User);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(r => r.Status == status);
        }

        var returnRequests = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper kullan
        // Not: OrderNumber ve UserName AutoMapper'da zaten map ediliyor
        return _mapper.Map<IEnumerable<ReturnRequestDto>>(returnRequests);
    }

    public async Task<ReturnRequestDto> CreateAsync(CreateReturnRequestDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        if (dto.OrderItemIds == null || !dto.OrderItemIds.Any())
        {
            throw new ValidationException("İade edilecek ürün seçilmedi.");
        }

        if (string.IsNullOrWhiteSpace(dto.Reason))
        {
            throw new ValidationException("İade nedeni boş olamaz.");
        }

        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.UserId == dto.UserId);

        if (order == null)
        {
            throw new NotFoundException("Sipariş", dto.OrderId);
        }

        if (order.Status != "Delivered")
        {
            throw new BusinessException("Sadece teslim edilmiş siparişler için iade yapılabilir.");
        }

        // Teslim edildikten sonra 14 gün içinde iade yapılabilir
        if (order.DeliveredDate.HasValue && (DateTime.UtcNow - order.DeliveredDate.Value).TotalDays > 14)
        {
            throw new BusinessException("İade süresi dolmuş. Teslim tarihinden itibaren 14 gün içinde iade yapılabilir.");
        }

        // ✅ PERFORMANCE: Database'de Sum yap (ToListAsync() sonrası Sum YASAK)
        // İade edilecek kalemlerin toplam tutarını hesapla
        var refundAmount = await _context.OrderItems
            .Where(oi => oi.OrderId == dto.OrderId && dto.OrderItemIds.Contains(oi.Id))
            .SumAsync(oi => (decimal?)oi.TotalPrice) ?? 0;

        if (refundAmount == 0)
        {
            throw new ValidationException("İade edilecek ürün seçilmedi.");
        }

        var returnRequest = new ReturnRequest
        {
            OrderId = dto.OrderId,
            UserId = dto.UserId,
            Reason = dto.Reason,
            Status = "Pending",
            RefundAmount = refundAmount,
            OrderItemIds = dto.OrderItemIds
        };

        returnRequest = await _returnRequestRepository.AddAsync(returnRequest);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with all includes in one query instead of multiple LoadAsync calls (N+1 fix)
        returnRequest = await _context.ReturnRequests
            .AsNoTracking()
            .Include(r => r.Order)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == returnRequest.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: OrderNumber ve UserName AutoMapper'da zaten map ediliyor
        return _mapper.Map<ReturnRequestDto>(returnRequest);
    }

    public async Task<ReturnRequestDto> UpdateStatusAsync(Guid id, string status, string? rejectionReason = null)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ValidationException("Durum boş olamaz.");
        }

        var returnRequest = await _returnRequestRepository.GetByIdAsync(id);
        if (returnRequest == null)
        {
            throw new NotFoundException("İade talebi", id);
        }

        returnRequest.Status = status;
        if (status == "Rejected" && !string.IsNullOrEmpty(rejectionReason))
        {
            returnRequest.RejectionReason = rejectionReason;
        }
        else if (status == "Approved")
        {
            returnRequest.ApprovedAt = DateTime.UtcNow;
        }
        else if (status == "Completed")
        {
            returnRequest.CompletedAt = DateTime.UtcNow;
        }

        await _returnRequestRepository.UpdateAsync(returnRequest);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with all includes in one query instead of multiple LoadAsync calls (N+1 fix)
        returnRequest = await _context.ReturnRequests
            .AsNoTracking()
            .Include(r => r.Order)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: OrderNumber ve UserName AutoMapper'da zaten map ediliyor
        return _mapper.Map<ReturnRequestDto>(returnRequest);
    }

    public async Task<bool> ApproveAsync(Guid id)
    {
        var returnRequest = await _returnRequestRepository.GetByIdAsync(id);
        if (returnRequest == null)
        {
            return false;
        }

        returnRequest.Status = "Approved";
        returnRequest.ApprovedAt = DateTime.UtcNow;
        await _returnRequestRepository.UpdateAsync(returnRequest);
        await _unitOfWork.SaveChangesAsync();

        // Ödeme iadesi yapılacak (Payment service ile entegre edilecek)
        // Burada sadece status güncelleniyor

        return true;
    }

    public async Task<bool> RejectAsync(Guid id, string reason)
    {
        return await UpdateStatusAsync(id, "Rejected", reason) != null;
    }

    public async Task<bool> CompleteAsync(Guid id, string trackingNumber)
    {
        var returnRequest = await _returnRequestRepository.GetByIdAsync(id);
        if (returnRequest == null)
        {
            return false;
        }

        returnRequest.Status = "Completed";
        returnRequest.CompletedAt = DateTime.UtcNow;
        returnRequest.TrackingNumber = trackingNumber;
        await _returnRequestRepository.UpdateAsync(returnRequest);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

