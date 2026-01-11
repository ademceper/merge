using AutoMapper;
using OrderEntity = Merge.Domain.Entities.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Order;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Application.DTOs.Order;
using Merge.Application.Common;


namespace Merge.Application.Services.Order;

public class ReturnRequestService : IReturnRequestService
{
    private readonly IRepository<ReturnRequest> _returnRequestRepository;
    private readonly IRepository<OrderEntity> _orderRepository;
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReturnRequestService> _logger;

    public ReturnRequestService(
        IRepository<ReturnRequest> returnRequestRepository,
        IRepository<OrderEntity> orderRepository,
        IDbContext context,
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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ReturnRequestDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var returnRequest = await _context.Set<ReturnRequest>()
            .AsNoTracking()
            .Include(r => r.Order)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (returnRequest == null) return null;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: OrderNumber ve UserName AutoMapper'da zaten map ediliyor
        return _mapper.Map<ReturnRequestDto>(returnRequest);
    }

    // ✅ PERFORMANCE: Pagination eklendi - unbounded query önleme
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PagedResult<ReturnRequestDto>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100; // Max limit
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var query = _context.Set<ReturnRequest>()
            .AsNoTracking()
            .Include(r => r.Order)
            .Include(r => r.User)
            .Where(r => r.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var returnRequests = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper kullan
        // Not: OrderNumber ve UserName AutoMapper'da zaten map ediliyor
        var dtos = _mapper.Map<IEnumerable<ReturnRequestDto>>(returnRequests);

        return new PagedResult<ReturnRequestDto>
        {
            Items = dtos.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PagedResult<ReturnRequestDto>> GetAllAsync(string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100; // Max limit
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        IQueryable<ReturnRequest> query = _context.Set<ReturnRequest>()
            .AsNoTracking()
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

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper kullan
        // Not: OrderNumber ve UserName AutoMapper'da zaten map ediliyor
        var dtos = _mapper.Map<IEnumerable<ReturnRequestDto>>(returnRequests);

        return new PagedResult<ReturnRequestDto>
        {
            Items = dtos.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ReturnRequestDto> CreateAsync(CreateReturnRequestDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Return request oluşturuluyor. OrderId: {OrderId}, UserId: {UserId}, Reason: {Reason}",
            dto.OrderId, dto.UserId, dto.Reason);

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
        var order = await _context.Set<OrderEntity>()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.UserId == dto.UserId, cancellationToken);

        if (order == null)
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

        // ✅ PERFORMANCE: Database'de Sum yap (ToListAsync() sonrası Sum YASAK)
        // İade edilecek kalemlerin toplam tutarını hesapla
        var refundAmount = await _context.Set<OrderItem>()
            .Where(oi => oi.OrderId == dto.OrderId && dto.OrderItemIds.Contains(oi.Id))
            .SumAsync(oi => (decimal?)oi.TotalPrice, cancellationToken) ?? 0;

        if (refundAmount == 0)
        {
            throw new ValidationException("İade edilecek ürün seçilmedi.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        // Order zaten yukarıda query edildi, tekrar query etme
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == dto.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", dto.UserId);
        }

        var returnRequest = ReturnRequest.Create(
            dto.OrderId,
            dto.UserId,
            dto.Reason,
            refundAmount,
            dto.OrderItemIds.ToList(),
            order,
            user);

        returnRequest = await _returnRequestRepository.AddAsync(returnRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with all includes in one query instead of multiple LoadAsync calls (N+1 fix)
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

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Return request oluşturuldu. ReturnRequestId: {ReturnRequestId}, OrderId: {OrderId}, RefundAmount: {RefundAmount}",
            reloadedReturnRequest.Id, dto.OrderId, refundAmount);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: OrderNumber ve UserName AutoMapper'da zaten map ediliyor
        return _mapper.Map<ReturnRequestDto>(reloadedReturnRequest);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ReturnRequestDto> UpdateStatusAsync(Guid id, string status, string? rejectionReason = null, CancellationToken cancellationToken = default)
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

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
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

        await _returnRequestRepository.UpdateAsync(returnRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with all includes in one query instead of multiple LoadAsync calls (N+1 fix)
        returnRequest = await _context.Set<ReturnRequest>()
            .AsNoTracking()
            .Include(r => r.Order)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: OrderNumber ve UserName AutoMapper'da zaten map ediliyor
        return _mapper.Map<ReturnRequestDto>(returnRequest);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> ApproveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var returnRequest = await _returnRequestRepository.GetByIdAsync(id);
        if (returnRequest == null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        returnRequest.Approve();
        await _returnRequestRepository.UpdateAsync(returnRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Ödeme iadesi yapılacak (Payment service ile entegre edilecek)
        // Burada sadece status güncelleniyor

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> RejectAsync(Guid id, string reason, CancellationToken cancellationToken = default)
    {
        return await UpdateStatusAsync(id, "Rejected", reason, cancellationToken) != null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> CompleteAsync(Guid id, string trackingNumber, CancellationToken cancellationToken = default)
    {
        var returnRequest = await _returnRequestRepository.GetByIdAsync(id);
        if (returnRequest == null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        returnRequest.Complete(trackingNumber);
        await _returnRequestRepository.UpdateAsync(returnRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

