using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Security;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using PaymentEntity = Merge.Domain.Entities.Payment;
using OrderEntity = Merge.Domain.Entities.Order;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Application.DTOs.Security;
using Merge.Application.Common;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
namespace Merge.Application.Services.Security;

public class OrderVerificationService : IOrderVerificationService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderVerificationService> _logger;
    private readonly ServiceSettings _serviceSettings;

    public OrderVerificationService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<OrderVerificationService> logger, IOptions<ServiceSettings> serviceSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _serviceSettings = serviceSettings.Value;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<OrderVerificationDto> CreateVerificationAsync(CreateOrderVerificationDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Order verification oluşturuluyor. OrderId: {OrderId}, VerificationType: {VerificationType}",
            dto.OrderId, dto.VerificationType);

        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        var order = await _context.Set<OrderEntity>()
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException("Sipariş", dto.OrderId);
        }

        // ✅ PERFORMANCE: Removed manual !v.IsDeleted (Global Query Filter)
        // Check if verification already exists
        var existing = await _context.Set<OrderVerification>()
            .FirstOrDefaultAsync(v => v.OrderId == dto.OrderId, cancellationToken);

        if (existing != null)
        {
            throw new BusinessException("Bu sipariş için zaten bir doğrulama kaydı var.");
        }

        // Calculate risk score (simplified)
        var riskScore = await CalculateOrderRiskScoreAsync(dto.OrderId, cancellationToken);

        var verification = new OrderVerification
        {
            OrderId = dto.OrderId,
            VerificationType = dto.VerificationType,
            Status = VerificationStatus.Pending,
            VerificationMethod = dto.VerificationMethod,
            VerificationNotes = dto.VerificationNotes,
            RequiresManualReview = dto.RequiresManualReview || riskScore >= 70,
            RiskScore = riskScore
        };

        await _context.Set<OrderVerification>().AddAsync(verification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        verification = await _context.Set<OrderVerification>()
            .AsNoTracking()
            .Include(v => v.Order)
            .Include(v => v.VerifiedBy)
            .FirstOrDefaultAsync(v => v.Id == verification.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Order verification oluşturuldu. VerificationId: {VerificationId}, OrderId: {OrderId}, RiskScore: {RiskScore}",
            verification!.Id, dto.OrderId, riskScore);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<OrderVerificationDto>(verification);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<OrderVerificationDto?> GetVerificationByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !v.IsDeleted (Global Query Filter)
        var verification = await _context.Set<OrderVerification>()
            .AsNoTracking()
            .Include(v => v.Order)
            .Include(v => v.VerifiedBy)
            .FirstOrDefaultAsync(v => v.OrderId == orderId, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return verification != null ? _mapper.Map<OrderVerificationDto>(verification) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<OrderVerificationDto>> GetPendingVerificationsAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !v.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Include already loaded, no need for MapToDto LoadAsync calls
        var verifications = await _context.Set<OrderVerification>()
            .AsNoTracking()
            .Include(v => v.Order)
            .Include(v => v.VerifiedBy)
            .Where(v => v.Status == VerificationStatus.Pending)
            .OrderByDescending(v => v.RequiresManualReview)
            .ThenByDescending(v => v.RiskScore)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<OrderVerificationDto>>(verifications);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<bool> VerifyOrderAsync(Guid verificationId, Guid verifiedByUserId, string? notes = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !v.IsDeleted (Global Query Filter)
        var verification = await _context.Set<OrderVerification>()
            .FirstOrDefaultAsync(v => v.Id == verificationId, cancellationToken);

        if (verification == null) return false;

        verification.Status = VerificationStatus.Verified;
        verification.VerifiedByUserId = verifiedByUserId;
        verification.VerifiedAt = DateTime.UtcNow;
        verification.VerificationNotes = notes;
        verification.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Order verified. VerificationId: {VerificationId}, VerifiedByUserId: {VerifiedByUserId}",
            verificationId, verifiedByUserId);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<bool> RejectOrderAsync(Guid verificationId, Guid verifiedByUserId, string reason, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !v.IsDeleted (Global Query Filter)
        var verification = await _context.Set<OrderVerification>()
            .FirstOrDefaultAsync(v => v.Id == verificationId, cancellationToken);

        if (verification == null) return false;

        verification.Status = VerificationStatus.Rejected;
        verification.VerifiedByUserId = verifiedByUserId;
        verification.VerifiedAt = DateTime.UtcNow;
        verification.RejectionReason = reason;
        verification.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Order rejected. VerificationId: {VerificationId}, VerifiedByUserId: {VerifiedByUserId}, Reason: {Reason}",
            verificationId, verifiedByUserId, reason);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<OrderVerificationDto>> GetAllVerificationsAsync(string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !v.IsDeleted (Global Query Filter)
        // ✅ FIX: Explicitly type as IQueryable to avoid IIncludableQueryable type mismatch
        IQueryable<OrderVerification> query = _context.Set<OrderVerification>()
            .AsNoTracking()
            .Include(v => v.Order)
            .Include(v => v.VerifiedBy);

        if (!string.IsNullOrEmpty(status))
        {
            var statusEnum = Enum.Parse<VerificationStatus>(status);
            query = query.Where(v => v.Status == statusEnum);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var verifications = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Include already loaded, MapToDto is now sync
        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var verificationDtos = _mapper.Map<IEnumerable<OrderVerificationDto>>(verifications).ToList();

        return new PagedResult<OrderVerificationDto>
        {
            Items = verificationDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    private async Task<int> CalculateOrderRiskScoreAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !o.IsDeleted (Global Query Filter)
        var order = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null) return 0;

        int riskScore = 0;

        // High value order
        if (order.TotalAmount > 10000) riskScore += 30;

        // New user
        var daysSinceRegistration = (DateTime.UtcNow - order.User.CreatedAt).Days;
        if (daysSinceRegistration < 7) riskScore += 20;

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // Multiple items
        var itemCount = await _context.Set<OrderItem>()
            .AsNoTracking()
            .CountAsync(oi => oi.OrderId == orderId, cancellationToken);
        if (itemCount > 10) riskScore += 15;

        // High quantity
        var totalQuantity = await _context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => oi.OrderId == orderId)
            .SumAsync(oi => oi.Quantity, cancellationToken);
        if (totalQuantity > 20) riskScore += 15;

        return Math.Min(riskScore, 100);
    }

}

public class PaymentFraudPreventionService : IPaymentFraudPreventionService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentFraudPreventionService> _logger;

    public PaymentFraudPreventionService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<PaymentFraudPreventionService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<PaymentFraudPreventionDto> CheckPaymentAsync(CreatePaymentFraudCheckDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Payment fraud check yapılıyor. PaymentId: {PaymentId}, CheckType: {CheckType}",
            dto.PaymentId, dto.CheckType);

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var payment = await _context.Set<PaymentEntity>()
            .FirstOrDefaultAsync(p => p.Id == dto.PaymentId, cancellationToken);

        if (payment == null)
        {
            throw new NotFoundException("Ödeme", Guid.Empty);
        }

        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        // Check if check already exists
        var existing = await _context.Set<PaymentFraudPrevention>()
            .FirstOrDefaultAsync(c => c.PaymentId == dto.PaymentId, cancellationToken);

        if (existing != null)
        {
            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            existing = await _context.Set<PaymentFraudPrevention>()
                .AsNoTracking()
                .Include(c => c.Payment)
                .FirstOrDefaultAsync(c => c.Id == existing.Id, cancellationToken);
            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return _mapper.Map<PaymentFraudPreventionDto>(existing!);
        }

        // Perform fraud checks
        var riskScore = await PerformFraudChecksAsync(dto, cancellationToken);
        var isBlocked = riskScore >= 70;
        var status = isBlocked ? "Blocked" : (riskScore >= 50 ? "Failed" : "Passed");

        var check = new PaymentFraudPrevention
        {
            PaymentId = dto.PaymentId,
            CheckType = dto.CheckType,
            Status = Enum.Parse<VerificationStatus>(status),
            IsBlocked = isBlocked,
            BlockReason = isBlocked ? $"High risk score: {riskScore}" : null,
            RiskScore = riskScore,
            CheckResult = JsonSerializer.Serialize(new { RiskScore = riskScore, CheckType = dto.CheckType }),
            CheckedAt = DateTime.UtcNow,
            DeviceFingerprint = dto.DeviceFingerprint,
            IpAddress = dto.IpAddress,
            UserAgent = dto.UserAgent
        };

        await _context.Set<PaymentFraudPrevention>().AddAsync(check, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        check = await _context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .Include(c => c.Payment)
            .FirstOrDefaultAsync(c => c.Id == check.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Payment fraud check tamamlandı. CheckId: {CheckId}, PaymentId: {PaymentId}, RiskScore: {RiskScore}, IsBlocked: {IsBlocked}",
            check!.Id, dto.PaymentId, riskScore, isBlocked);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<PaymentFraudPreventionDto>(check);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PaymentFraudPreventionDto?> GetCheckByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var check = await _context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .Include(c => c.Payment)
            .FirstOrDefaultAsync(c => c.PaymentId == paymentId, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return check != null ? _mapper.Map<PaymentFraudPreventionDto>(check) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<PaymentFraudPreventionDto>> GetBlockedPaymentsAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Include already loaded, MapToDto is now sync
        var checks = await _context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .Include(c => c.Payment)
            .Where(c => c.IsBlocked)
            .OrderByDescending(c => c.RiskScore)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<PaymentFraudPreventionDto>>(checks);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<bool> BlockPaymentAsync(Guid checkId, string reason, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var check = await _context.Set<PaymentFraudPrevention>()
            .FirstOrDefaultAsync(c => c.Id == checkId, cancellationToken);

        if (check == null) return false;

        check.IsBlocked = true;
        check.BlockReason = reason;
        check.Status = VerificationStatus.Failed; // Blocked -> Failed
        check.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Payment blocked. CheckId: {CheckId}, Reason: {Reason}", checkId, reason);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<bool> UnblockPaymentAsync(Guid checkId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var check = await _context.Set<PaymentFraudPrevention>()
            .FirstOrDefaultAsync(c => c.Id == checkId, cancellationToken);

        if (check == null) return false;

        check.IsBlocked = false;
        check.BlockReason = null;
        check.Status = VerificationStatus.Verified; // Passed -> Verified
        check.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Payment unblocked. CheckId: {CheckId}", checkId);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<PaymentFraudPreventionDto>> GetAllChecksAsync(string? status = null, bool? isBlocked = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        // ✅ FIX: Explicitly type as IQueryable to avoid IIncludableQueryable type mismatch
        IQueryable<PaymentFraudPrevention> query = _context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .Include(c => c.Payment);

        if (!string.IsNullOrEmpty(status))
        {
            var statusEnum = Enum.Parse<VerificationStatus>(status);
            query = query.Where(c => c.Status == statusEnum);
        }

        if (isBlocked.HasValue)
        {
            query = query.Where(c => c.IsBlocked == isBlocked.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var checks = await query
            .OrderByDescending(c => c.RiskScore)
            .ThenByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Include already loaded, MapToDto is now sync
        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var checkDtos = _mapper.Map<IEnumerable<PaymentFraudPreventionDto>>(checks).ToList();

        return new PagedResult<PaymentFraudPreventionDto>
        {
            Items = checkDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    private async Task<int> PerformFraudChecksAsync(CreatePaymentFraudCheckDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var payment = await _context.Set<PaymentEntity>()
            .AsNoTracking()
            .Include(p => p.Order)
                .ThenInclude(o => o.User)
            .FirstOrDefaultAsync(p => p.Id == dto.PaymentId, cancellationToken);

        if (payment == null) return 0;

        int riskScore = 0;

        // High value payment
        if (payment.Amount > 5000) riskScore += 25;

        // New user
        var daysSinceRegistration = (DateTime.UtcNow - payment.Order.User.CreatedAt).Days;
        if (daysSinceRegistration < 7) riskScore += 20;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        // Multiple payments from same IP in short time
        var recentPayments = await _context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .Where(c => c.IpAddress == dto.IpAddress && c.CreatedAt >= DateTime.UtcNow.AddHours(-1)) // ✅ BOLUM 12.0: ShortDateRangeDays kullanılabilir ama bu özel durum (1 saat)
            .CountAsync(cancellationToken);

        if (recentPayments > 3) riskScore += 30;

        // Device fingerprint check
        if (string.IsNullOrEmpty(dto.DeviceFingerprint)) riskScore += 15;

        return Math.Min(riskScore, 100);
    }

}

public class AccountSecurityMonitoringService : IAccountSecurityMonitoringService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AccountSecurityMonitoringService> _logger;
    private readonly ServiceSettings _serviceSettings;

    public AccountSecurityMonitoringService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<AccountSecurityMonitoringService> logger, IOptions<ServiceSettings> serviceSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _serviceSettings = serviceSettings.Value;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<AccountSecurityEventDto> LogSecurityEventAsync(CreateAccountSecurityEventDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Security event loglanıyor. UserId: {UserId}, EventType: {EventType}, Severity: {Severity}",
            dto.UserId, dto.EventType, dto.Severity);

        // ✅ PERFORMANCE: Removed manual !u.IsDeleted (Global Query Filter)
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == dto.UserId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", Guid.Empty);
        }

        var securityEvent = new AccountSecurityEvent
        {
            UserId = dto.UserId,
            EventType = dto.EventType,
            Severity = dto.Severity,
            IpAddress = dto.IpAddress,
            UserAgent = dto.UserAgent,
            Location = dto.Location,
            DeviceFingerprint = dto.DeviceFingerprint,
            IsSuspicious = dto.IsSuspicious,
            Details = dto.Details != null ? JsonSerializer.Serialize(dto.Details) : null,
            RequiresAction = dto.RequiresAction
        };

        await _context.Set<AccountSecurityEvent>().AddAsync(securityEvent, cancellationToken);

        // If suspicious, create alert
        if (dto.IsSuspicious || dto.RequiresAction)
        {
            var alert = new SecurityAlert
            {
                UserId = dto.UserId,
                AlertType = "Account",
                Severity = dto.Severity == "Critical" ? "Critical" : "High",
                Title = $"Suspicious activity detected: {dto.EventType}",
                Description = $"Security event: {dto.EventType} for user {user.Email}",
                Status = AlertStatus.New,
                Metadata = dto.Details != null ? JsonSerializer.Serialize(dto.Details) : null
            };
            await _context.Set<SecurityAlert>().AddAsync(alert, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        securityEvent = await _context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .Include(e => e.User)
            .Include(e => e.ActionTakenBy)
            .FirstOrDefaultAsync(e => e.Id == securityEvent.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Security event loglandı. EventId: {EventId}, UserId: {UserId}, EventType: {EventType}",
            securityEvent!.Id, dto.UserId, dto.EventType);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<AccountSecurityEventDto>(securityEvent);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<AccountSecurityEventDto>> GetUserSecurityEventsAsync(Guid userId, string? eventType = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !e.IsDeleted (Global Query Filter)
        var query = _context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .Include(e => e.User)
            .Include(e => e.ActionTakenBy)
            .Where(e => e.UserId == userId);

        if (!string.IsNullOrEmpty(eventType))
        {
            query = query.Where(e => e.EventType == eventType);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var events = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var eventDtos = _mapper.Map<IEnumerable<AccountSecurityEventDto>>(events).ToList();

        return new PagedResult<AccountSecurityEventDto>
        {
            Items = eventDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<AccountSecurityEventDto>> GetSuspiciousEventsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !e.IsDeleted (Global Query Filter)
        var query = _context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .Include(e => e.User)
            .Include(e => e.ActionTakenBy)
            .Where(e => e.IsSuspicious);

        var totalCount = await query.CountAsync(cancellationToken);

        var events = await query
            .OrderByDescending(e => e.Severity == "Critical")
            .ThenByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var eventDtos = _mapper.Map<IEnumerable<AccountSecurityEventDto>>(events).ToList();

        return new PagedResult<AccountSecurityEventDto>
        {
            Items = eventDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<bool> TakeActionAsync(Guid eventId, Guid actionTakenByUserId, string action, string? notes = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted (Global Query Filter)
        var securityEvent = await _context.Set<AccountSecurityEvent>()
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

        if (securityEvent == null) return false;

        securityEvent.ActionTaken = action;
        securityEvent.ActionTakenByUserId = actionTakenByUserId;
        securityEvent.ActionTakenAt = DateTime.UtcNow;
        securityEvent.RequiresAction = false;
        securityEvent.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Security event action alındı. EventId: {EventId}, Action: {Action}, ActionTakenByUserId: {ActionTakenByUserId}",
            eventId, action, actionTakenByUserId);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<SecurityAlertDto> CreateSecurityAlertAsync(CreateSecurityAlertDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Security alert oluşturuluyor. UserId: {UserId}, AlertType: {AlertType}, Severity: {Severity}",
            dto.UserId, dto.AlertType, dto.Severity);

        var alert = new SecurityAlert
        {
            UserId = dto.UserId,
            AlertType = dto.AlertType,
            Severity = dto.Severity,
            Title = dto.Title,
            Description = dto.Description,
            Status = AlertStatus.New,
            Metadata = dto.Metadata != null ? JsonSerializer.Serialize(dto.Metadata) : null
        };

        await _context.Set<SecurityAlert>().AddAsync(alert, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        alert = await _context.Set<SecurityAlert>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.AcknowledgedBy)
            .Include(a => a.ResolvedBy)
            .FirstOrDefaultAsync(a => a.Id == alert.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Security alert oluşturuldu. AlertId: {AlertId}, UserId: {UserId}", alert!.Id, dto.UserId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<SecurityAlertDto>(alert);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<SecurityAlertDto>> GetSecurityAlertsAsync(Guid? userId = null, string? severity = null, string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        // ✅ FIX: Explicitly type as IQueryable to avoid IIncludableQueryable type mismatch
        IQueryable<SecurityAlert> query = _context.Set<SecurityAlert>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.AcknowledgedBy)
            .Include(a => a.ResolvedBy);

        if (userId.HasValue)
        {
            query = query.Where(a => a.UserId == userId.Value);
        }

        if (!string.IsNullOrEmpty(severity))
        {
            query = query.Where(a => a.Severity == severity);
        }

        if (!string.IsNullOrEmpty(status))
        {
            var statusEnum = Enum.Parse<AlertStatus>(status);
            query = query.Where(a => a.Status == statusEnum);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var alerts = await query
            .OrderByDescending(a => a.Severity == "Critical")
            .ThenByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var alertDtos = _mapper.Map<IEnumerable<SecurityAlertDto>>(alerts).ToList();

        return new PagedResult<SecurityAlertDto>
        {
            Items = alertDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<bool> AcknowledgeAlertAsync(Guid alertId, Guid acknowledgedByUserId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var alert = await _context.Set<SecurityAlert>()
            .FirstOrDefaultAsync(a => a.Id == alertId, cancellationToken);

        if (alert == null) return false;

        alert.Status = AlertStatus.Acknowledged;
        alert.AcknowledgedByUserId = acknowledgedByUserId;
        alert.AcknowledgedAt = DateTime.UtcNow;
        alert.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Security alert acknowledged. AlertId: {AlertId}, AcknowledgedByUserId: {AcknowledgedByUserId}",
            alertId, acknowledgedByUserId);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<bool> ResolveAlertAsync(Guid alertId, Guid resolvedByUserId, string? resolutionNotes = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var alert = await _context.Set<SecurityAlert>()
            .FirstOrDefaultAsync(a => a.Id == alertId, cancellationToken);

        if (alert == null) return false;

        alert.Status = AlertStatus.Resolved;
        alert.ResolvedByUserId = resolvedByUserId;
        alert.ResolvedAt = DateTime.UtcNow;
        alert.ResolutionNotes = resolutionNotes;
        alert.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Security alert resolved. AlertId: {AlertId}, ResolvedByUserId: {ResolvedByUserId}",
            alertId, resolvedByUserId);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SecurityMonitoringSummaryDto> GetSecuritySummaryAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-_serviceSettings.DefaultDateRangeDays); // ✅ BOLUM 12.0: Magic number config'den
        var end = endDate ?? DateTime.UtcNow;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !e.IsDeleted and !a.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var totalEvents = await _context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .Where(e => e.CreatedAt >= start && e.CreatedAt <= end)
            .CountAsync(cancellationToken);

        var suspiciousEvents = await _context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .Where(e => e.CreatedAt >= start && e.CreatedAt <= end && e.IsSuspicious)
            .CountAsync(cancellationToken);

        var criticalEvents = await _context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .Where(e => e.CreatedAt >= start && e.CreatedAt <= end && e.Severity == "Critical")
            .CountAsync(cancellationToken);

        var pendingAlerts = await _context.Set<SecurityAlert>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end && a.Status == AlertStatus.New)
            .CountAsync(cancellationToken);

        var resolvedAlerts = await _context.Set<SecurityAlert>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end && a.Status == AlertStatus.Resolved)
            .CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        var eventsByType = await _context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .Where(e => e.CreatedAt >= start && e.CreatedAt <= end)
            .GroupBy(e => e.EventType)
            .Select(g => new { EventType = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.EventType, x => x.Count, cancellationToken);

        var alertsBySeverity = await _context.Set<SecurityAlert>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end)
            .GroupBy(a => a.Severity)
            .Select(g => new { Severity = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Severity, x => x.Count, cancellationToken);

        // ✅ PERFORMANCE: Database'de filtreleme/sıralama yap (memory'de işlem YASAK)
        var recentCriticalAlerts = await _context.Set<SecurityAlert>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.AcknowledgedBy)
            .Include(a => a.ResolvedBy)
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end && 
                       a.Severity == "Critical" && a.Status != AlertStatus.Resolved)
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var recentCriticalAlertsDtos = _mapper.Map<IEnumerable<SecurityAlertDto>>(recentCriticalAlerts).ToList();

        return new SecurityMonitoringSummaryDto
        {
            TotalSecurityEvents = totalEvents,
            SuspiciousEvents = suspiciousEvents,
            CriticalEvents = criticalEvents,
            PendingAlerts = pendingAlerts,
            ResolvedAlerts = resolvedAlerts,
            EventsByType = eventsByType,
            AlertsBySeverity = alertsBySeverity,
            RecentCriticalAlerts = recentCriticalAlertsDtos
        };
    }

}

