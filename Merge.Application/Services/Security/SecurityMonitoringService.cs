using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Security;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using System.Text.Json;
using Merge.Application.DTOs.Security;

namespace Merge.Application.Services.Security;

public class OrderVerificationService : IOrderVerificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderVerificationService> _logger;

    public OrderVerificationService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<OrderVerificationService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OrderVerificationDto> CreateVerificationAsync(CreateOrderVerificationDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

        if (order == null)
        {
            throw new NotFoundException("Sipariş", dto.OrderId);
        }

        // ✅ PERFORMANCE: Removed manual !v.IsDeleted (Global Query Filter)
        // Check if verification already exists
        var existing = await _context.Set<OrderVerification>()
            .FirstOrDefaultAsync(v => v.OrderId == dto.OrderId);

        if (existing != null)
        {
            throw new BusinessException("Bu sipariş için zaten bir doğrulama kaydı var.");
        }

        // Calculate risk score (simplified)
        var riskScore = await CalculateOrderRiskScoreAsync(dto.OrderId);

        var verification = new OrderVerification
        {
            OrderId = dto.OrderId,
            VerificationType = dto.VerificationType,
            Status = "Pending",
            VerificationMethod = dto.VerificationMethod,
            VerificationNotes = dto.VerificationNotes,
            RequiresManualReview = dto.RequiresManualReview || riskScore >= 70,
            RiskScore = riskScore
        };

        await _context.Set<OrderVerification>().AddAsync(verification);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        verification = await _context.Set<OrderVerification>()
            .AsNoTracking()
            .Include(v => v.Order)
            .Include(v => v.VerifiedBy)
            .FirstOrDefaultAsync(v => v.Id == verification.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<OrderVerificationDto>(verification!);
    }

    public async Task<OrderVerificationDto?> GetVerificationByOrderIdAsync(Guid orderId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !v.IsDeleted (Global Query Filter)
        var verification = await _context.Set<OrderVerification>()
            .AsNoTracking()
            .Include(v => v.Order)
            .Include(v => v.VerifiedBy)
            .FirstOrDefaultAsync(v => v.OrderId == orderId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return verification != null ? _mapper.Map<OrderVerificationDto>(verification) : null;
    }

    public async Task<IEnumerable<OrderVerificationDto>> GetPendingVerificationsAsync()
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !v.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Include already loaded, no need for MapToDto LoadAsync calls
        var verifications = await _context.Set<OrderVerification>()
            .AsNoTracking()
            .Include(v => v.Order)
            .Include(v => v.VerifiedBy)
            .Where(v => v.Status == "Pending")
            .OrderByDescending(v => v.RequiresManualReview)
            .ThenByDescending(v => v.RiskScore)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<OrderVerificationDto>>(verifications);
    }

    public async Task<bool> VerifyOrderAsync(Guid verificationId, Guid verifiedByUserId, string? notes = null)
    {
        // ✅ PERFORMANCE: Removed manual !v.IsDeleted (Global Query Filter)
        var verification = await _context.Set<OrderVerification>()
            .FirstOrDefaultAsync(v => v.Id == verificationId);

        if (verification == null) return false;

        verification.Status = "Verified";
        verification.VerifiedByUserId = verifiedByUserId;
        verification.VerifiedAt = DateTime.UtcNow;
        verification.VerificationNotes = notes;
        verification.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectOrderAsync(Guid verificationId, Guid verifiedByUserId, string reason)
    {
        // ✅ PERFORMANCE: Removed manual !v.IsDeleted (Global Query Filter)
        var verification = await _context.Set<OrderVerification>()
            .FirstOrDefaultAsync(v => v.Id == verificationId);

        if (verification == null) return false;

        verification.Status = "Rejected";
        verification.VerifiedByUserId = verifiedByUserId;
        verification.VerifiedAt = DateTime.UtcNow;
        verification.RejectionReason = reason;
        verification.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<OrderVerificationDto>> GetAllVerificationsAsync(string? status = null, int page = 1, int pageSize = 20)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !v.IsDeleted (Global Query Filter)
        // ✅ FIX: Explicitly type as IQueryable to avoid IIncludableQueryable type mismatch
        IQueryable<OrderVerification> query = _context.Set<OrderVerification>()
            .AsNoTracking()
            .Include(v => v.Order)
            .Include(v => v.VerifiedBy);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(v => v.Status == status);
        }

        var verifications = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // ✅ PERFORMANCE: Include already loaded, MapToDto is now sync
        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<OrderVerificationDto>>(verifications);
    }

    private async Task<int> CalculateOrderRiskScoreAsync(Guid orderId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !o.IsDeleted (Global Query Filter)
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return 0;

        int riskScore = 0;

        // High value order
        if (order.TotalAmount > 10000) riskScore += 30;

        // New user
        var daysSinceRegistration = (DateTime.UtcNow - order.User.CreatedAt).Days;
        if (daysSinceRegistration < 7) riskScore += 20;

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // Multiple items
        var itemCount = await _context.OrderItems
            .AsNoTracking()
            .CountAsync(oi => oi.OrderId == orderId);
        if (itemCount > 10) riskScore += 15;

        // High quantity
        var totalQuantity = await _context.OrderItems
            .AsNoTracking()
            .Where(oi => oi.OrderId == orderId)
            .SumAsync(oi => oi.Quantity);
        if (totalQuantity > 20) riskScore += 15;

        return Math.Min(riskScore, 100);
    }

}

public class PaymentFraudPreventionService : IPaymentFraudPreventionService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentFraudPreventionService> _logger;

    public PaymentFraudPreventionService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<PaymentFraudPreventionService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaymentFraudPreventionDto> CheckPaymentAsync(CreatePaymentFraudCheckDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == dto.PaymentId);

        if (payment == null)
        {
            throw new NotFoundException("Ödeme", Guid.Empty);
        }

        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        // Check if check already exists
        var existing = await _context.Set<PaymentFraudPrevention>()
            .FirstOrDefaultAsync(c => c.PaymentId == dto.PaymentId);

        if (existing != null)
        {
            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            existing = await _context.Set<PaymentFraudPrevention>()
                .AsNoTracking()
                .Include(c => c.Payment)
                .FirstOrDefaultAsync(c => c.Id == existing.Id);
            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return _mapper.Map<PaymentFraudPreventionDto>(existing!);
        }

        // Perform fraud checks
        var riskScore = await PerformFraudChecksAsync(dto);
        var isBlocked = riskScore >= 70;
        var status = isBlocked ? "Blocked" : (riskScore >= 50 ? "Failed" : "Passed");

        var check = new PaymentFraudPrevention
        {
            PaymentId = dto.PaymentId,
            CheckType = dto.CheckType,
            Status = status,
            IsBlocked = isBlocked,
            BlockReason = isBlocked ? $"High risk score: {riskScore}" : null,
            RiskScore = riskScore,
            CheckResult = JsonSerializer.Serialize(new { RiskScore = riskScore, CheckType = dto.CheckType }),
            CheckedAt = DateTime.UtcNow,
            DeviceFingerprint = dto.DeviceFingerprint,
            IpAddress = dto.IpAddress,
            UserAgent = dto.UserAgent
        };

        await _context.Set<PaymentFraudPrevention>().AddAsync(check);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        check = await _context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .Include(c => c.Payment)
            .FirstOrDefaultAsync(c => c.Id == check.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<PaymentFraudPreventionDto>(check!);
    }

    public async Task<PaymentFraudPreventionDto?> GetCheckByPaymentIdAsync(Guid paymentId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var check = await _context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .Include(c => c.Payment)
            .FirstOrDefaultAsync(c => c.PaymentId == paymentId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return check != null ? _mapper.Map<PaymentFraudPreventionDto>(check) : null;
    }

    public async Task<IEnumerable<PaymentFraudPreventionDto>> GetBlockedPaymentsAsync()
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Include already loaded, MapToDto is now sync
        var checks = await _context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .Include(c => c.Payment)
            .Where(c => c.IsBlocked)
            .OrderByDescending(c => c.RiskScore)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<PaymentFraudPreventionDto>>(checks);
    }

    public async Task<bool> BlockPaymentAsync(Guid checkId, string reason)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var check = await _context.Set<PaymentFraudPrevention>()
            .FirstOrDefaultAsync(c => c.Id == checkId);

        if (check == null) return false;

        check.IsBlocked = true;
        check.BlockReason = reason;
        check.Status = "Blocked";
        check.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnblockPaymentAsync(Guid checkId)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var check = await _context.Set<PaymentFraudPrevention>()
            .FirstOrDefaultAsync(c => c.Id == checkId);

        if (check == null) return false;

        check.IsBlocked = false;
        check.BlockReason = null;
        check.Status = "Passed";
        check.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<PaymentFraudPreventionDto>> GetAllChecksAsync(string? status = null, bool? isBlocked = null, int page = 1, int pageSize = 20)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        // ✅ FIX: Explicitly type as IQueryable to avoid IIncludableQueryable type mismatch
        IQueryable<PaymentFraudPrevention> query = _context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .Include(c => c.Payment);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(c => c.Status == status);
        }

        if (isBlocked.HasValue)
        {
            query = query.Where(c => c.IsBlocked == isBlocked.Value);
        }

        var checks = await query
            .OrderByDescending(c => c.RiskScore)
            .ThenByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // ✅ PERFORMANCE: Include already loaded, MapToDto is now sync
        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<PaymentFraudPreventionDto>>(checks);
    }

    private async Task<int> PerformFraudChecksAsync(CreatePaymentFraudCheckDto dto)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var payment = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Order)
                .ThenInclude(o => o.User)
            .FirstOrDefaultAsync(p => p.Id == dto.PaymentId);

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
            .Where(c => c.IpAddress == dto.IpAddress && c.CreatedAt >= DateTime.UtcNow.AddHours(-1))
            .CountAsync();

        if (recentPayments > 3) riskScore += 30;

        // Device fingerprint check
        if (string.IsNullOrEmpty(dto.DeviceFingerprint)) riskScore += 15;

        return Math.Min(riskScore, 100);
    }

}

public class AccountSecurityMonitoringService : IAccountSecurityMonitoringService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AccountSecurityMonitoringService> _logger;

    public AccountSecurityMonitoringService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<AccountSecurityMonitoringService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AccountSecurityEventDto> LogSecurityEventAsync(CreateAccountSecurityEventDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !u.IsDeleted (Global Query Filter)
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == dto.UserId);

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

        await _context.Set<AccountSecurityEvent>().AddAsync(securityEvent);

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
                Status = "New",
                Metadata = dto.Details != null ? JsonSerializer.Serialize(dto.Details) : null
            };
            await _context.Set<SecurityAlert>().AddAsync(alert);
        }

        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        securityEvent = await _context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .Include(e => e.User)
            .Include(e => e.ActionTakenBy)
            .FirstOrDefaultAsync(e => e.Id == securityEvent.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<AccountSecurityEventDto>(securityEvent!);
    }

    public async Task<IEnumerable<AccountSecurityEventDto>> GetUserSecurityEventsAsync(Guid userId, string? eventType = null, int page = 1, int pageSize = 20)
    {
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

        var events = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<AccountSecurityEventDto>>(events);
    }

    public async Task<IEnumerable<AccountSecurityEventDto>> GetSuspiciousEventsAsync(int page = 1, int pageSize = 20)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !e.IsDeleted (Global Query Filter)
        var events = await _context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .Include(e => e.User)
            .Include(e => e.ActionTakenBy)
            .Where(e => e.IsSuspicious)
            .OrderByDescending(e => e.Severity == "Critical")
            .ThenByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<AccountSecurityEventDto>>(events);
    }

    public async Task<bool> TakeActionAsync(Guid eventId, Guid actionTakenByUserId, string action, string? notes = null)
    {
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted (Global Query Filter)
        var securityEvent = await _context.Set<AccountSecurityEvent>()
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (securityEvent == null) return false;

        securityEvent.ActionTaken = action;
        securityEvent.ActionTakenByUserId = actionTakenByUserId;
        securityEvent.ActionTakenAt = DateTime.UtcNow;
        securityEvent.RequiresAction = false;
        securityEvent.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<SecurityAlertDto> CreateSecurityAlertAsync(CreateSecurityAlertDto dto)
    {
        var alert = new SecurityAlert
        {
            UserId = dto.UserId,
            AlertType = dto.AlertType,
            Severity = dto.Severity,
            Title = dto.Title,
            Description = dto.Description,
            Status = "New",
            Metadata = dto.Metadata != null ? JsonSerializer.Serialize(dto.Metadata) : null
        };

        await _context.Set<SecurityAlert>().AddAsync(alert);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        alert = await _context.Set<SecurityAlert>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.AcknowledgedBy)
            .Include(a => a.ResolvedBy)
            .FirstOrDefaultAsync(a => a.Id == alert.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<SecurityAlertDto>(alert!);
    }

    public async Task<IEnumerable<SecurityAlertDto>> GetSecurityAlertsAsync(Guid? userId = null, string? severity = null, string? status = null, int page = 1, int pageSize = 20)
    {
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
            query = query.Where(a => a.Status == status);
        }

        var alerts = await query
            .OrderByDescending(a => a.Severity == "Critical")
            .ThenByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<SecurityAlertDto>>(alerts);
    }

    public async Task<bool> AcknowledgeAlertAsync(Guid alertId, Guid acknowledgedByUserId)
    {
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var alert = await _context.Set<SecurityAlert>()
            .FirstOrDefaultAsync(a => a.Id == alertId);

        if (alert == null) return false;

        alert.Status = "Acknowledged";
        alert.AcknowledgedByUserId = acknowledgedByUserId;
        alert.AcknowledgedAt = DateTime.UtcNow;
        alert.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResolveAlertAsync(Guid alertId, Guid resolvedByUserId, string? resolutionNotes = null)
    {
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var alert = await _context.Set<SecurityAlert>()
            .FirstOrDefaultAsync(a => a.Id == alertId);

        if (alert == null) return false;

        alert.Status = "Resolved";
        alert.ResolvedByUserId = resolvedByUserId;
        alert.ResolvedAt = DateTime.UtcNow;
        alert.ResolutionNotes = resolutionNotes;
        alert.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<SecurityMonitoringSummaryDto> GetSecuritySummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !e.IsDeleted and !a.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var totalEvents = await _context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .Where(e => e.CreatedAt >= start && e.CreatedAt <= end)
            .CountAsync();

        var suspiciousEvents = await _context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .Where(e => e.CreatedAt >= start && e.CreatedAt <= end && e.IsSuspicious)
            .CountAsync();

        var criticalEvents = await _context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .Where(e => e.CreatedAt >= start && e.CreatedAt <= end && e.Severity == "Critical")
            .CountAsync();

        var pendingAlerts = await _context.Set<SecurityAlert>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end && a.Status == "New")
            .CountAsync();

        var resolvedAlerts = await _context.Set<SecurityAlert>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end && a.Status == "Resolved")
            .CountAsync();

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        var eventsByType = await _context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .Where(e => e.CreatedAt >= start && e.CreatedAt <= end)
            .GroupBy(e => e.EventType)
            .Select(g => new { EventType = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.EventType, x => x.Count);

        var alertsBySeverity = await _context.Set<SecurityAlert>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end)
            .GroupBy(a => a.Severity)
            .Select(g => new { Severity = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Severity, x => x.Count);

        // ✅ PERFORMANCE: Database'de filtreleme/sıralama yap (memory'de işlem YASAK)
        var recentCriticalAlerts = await _context.Set<SecurityAlert>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.AcknowledgedBy)
            .Include(a => a.ResolvedBy)
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end && 
                       a.Severity == "Critical" && a.Status != "Resolved")
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .ToListAsync();

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

