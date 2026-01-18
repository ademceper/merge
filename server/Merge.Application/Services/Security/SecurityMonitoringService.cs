using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Security;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using PaymentEntity = Merge.Domain.Modules.Payment.Payment;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Application.DTOs.Security;
using Merge.Application.Common;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Services.Security;

/// <summary>
/// ⚠️ DEPRECATED: Bu service layer artık kullanılmıyor.
/// Tüm işlevsellik MediatR handlers'a taşınmıştır (CQRS pattern).
/// Bu dosya sadece referans amaçlı tutulmaktadır ve gelecekte silinebilir.
/// Yeni kod için Merge.Application.Security.Commands ve Merge.Application.Security.Queries kullanın.
/// </summary>
[Obsolete("This service is deprecated. Use MediatR commands and queries instead. See Merge.Application.Security.Commands and Merge.Application.Security.Queries.")]
public class OrderVerificationService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<OrderVerificationService> logger, IOptions<ServiceSettings> serviceSettings) : IOrderVerificationService
{
    private readonly ServiceSettings serviceConfig = serviceSettings.Value;

    public async Task<OrderVerificationDto> CreateVerificationAsync(CreateOrderVerificationDto dto, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Order verification oluşturuluyor. OrderId: {OrderId}, VerificationType: {VerificationType}",
            dto.OrderId, dto.VerificationType);

        var order = await context.Set<OrderEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException("Sipariş", dto.OrderId);
        }

        // Check if verification already exists
        var existing = await context.Set<OrderVerification>()
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.OrderId == dto.OrderId, cancellationToken);

        if (existing is not null)
        {
            throw new BusinessException("Bu sipariş için zaten bir doğrulama kaydı var.");
        }

        // Calculate risk score (simplified)
        var riskScore = await CalculateOrderRiskScoreAsync(dto.OrderId, cancellationToken);

        var verificationType = Enum.TryParse<VerificationType>(dto.VerificationType, true, out var parsedType)
            ? parsedType
            : VerificationType.Manual;

        var verification = OrderVerification.Create(
            orderId: dto.OrderId,
            verificationType: verificationType,
            riskScore: riskScore,
            verificationMethod: dto.VerificationMethod,
            verificationNotes: dto.VerificationNotes,
            requiresManualReview: dto.RequiresManualReview || riskScore >= 70);

        await context.Set<OrderVerification>().AddAsync(verification, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        verification = await context.Set<OrderVerification>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(v => v.Order)
            .Include(v => v.VerifiedBy)
            .FirstOrDefaultAsync(v => v.Id == verification.Id, cancellationToken);

        logger.LogInformation("Order verification oluşturuldu. VerificationId: {VerificationId}, OrderId: {OrderId}, RiskScore: {RiskScore}",
            verification!.Id, dto.OrderId, riskScore);

        return mapper.Map<OrderVerificationDto>(verification);
    }

    public async Task<OrderVerificationDto?> GetVerificationByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var verification = await context.Set<OrderVerification>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(v => v.Order)
            .Include(v => v.VerifiedBy)
            .FirstOrDefaultAsync(v => v.OrderId == orderId, cancellationToken);

        return verification is not null ? mapper.Map<OrderVerificationDto>(verification) : null;
    }

    public async Task<IEnumerable<OrderVerificationDto>> GetPendingVerificationsAsync(CancellationToken cancellationToken = default)
    {
        var verifications = await context.Set<OrderVerification>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(v => v.Order)
            .Include(v => v.VerifiedBy)
            .Where(v => v.Status == VerificationStatus.Pending)
            .OrderByDescending(v => v.RequiresManualReview)
            .ThenByDescending(v => v.RiskScore)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<OrderVerificationDto>>(verifications);
    }

    public async Task<bool> VerifyOrderAsync(Guid verificationId, Guid verifiedByUserId, string? notes = null, CancellationToken cancellationToken = default)
    {
        var verification = await context.Set<OrderVerification>()
            .FirstOrDefaultAsync(v => v.Id == verificationId, cancellationToken);

        if (verification is null) return false;

        verification.Verify(verifiedByUserId, notes);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Order verified. VerificationId: {VerificationId}, VerifiedByUserId: {VerifiedByUserId}",
            verificationId, verifiedByUserId);

        return true;
    }

    public async Task<bool> RejectOrderAsync(Guid verificationId, Guid verifiedByUserId, string reason, CancellationToken cancellationToken = default)
    {
        var verification = await context.Set<OrderVerification>()
            .FirstOrDefaultAsync(v => v.Id == verificationId, cancellationToken);

        if (verification is null) return false;

        verification.Reject(verifiedByUserId, reason);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Order rejected. VerificationId: {VerificationId}, VerifiedByUserId: {VerifiedByUserId}, Reason: {Reason}",
            verificationId, verifiedByUserId, reason);

        return true;
    }

    public async Task<PagedResult<OrderVerificationDto>> GetAllVerificationsAsync(string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        IQueryable<OrderVerification> query = context.Set<OrderVerification>()
            .AsNoTracking()
            .AsSplitQuery()
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

        var verificationDtos = mapper.Map<IEnumerable<OrderVerificationDto>>(verifications).ToList();

        return new PagedResult<OrderVerificationDto>
        {
            Items = verificationDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private async Task<int> CalculateOrderRiskScoreAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await context.Set<OrderEntity>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.OrderItems)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null) return 0;

        int riskScore = 0;

        // High value order
        if (order.TotalAmount > RiskScoreConstants.HighValueOrderThreshold) 
            riskScore += RiskScoreConstants.HighValueOrderScore;

        // New user
        var daysSinceRegistration = (DateTime.UtcNow - order.User.CreatedAt).Days;
        if (daysSinceRegistration < RiskScoreConstants.NewUserDaysThreshold) 
            riskScore += RiskScoreConstants.NewUserScore;

        // Multiple items
        var itemCount = await context.Set<OrderItem>()
            .AsNoTracking()
            .CountAsync(oi => oi.OrderId == orderId, cancellationToken);
        if (itemCount > RiskScoreConstants.MultipleItemsThreshold) 
            riskScore += RiskScoreConstants.MultipleItemsScore;

        // High quantity
        var totalQuantity = await context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => oi.OrderId == orderId)
            .SumAsync(oi => oi.Quantity, cancellationToken);
        if (totalQuantity > RiskScoreConstants.HighQuantityThreshold) 
            riskScore += RiskScoreConstants.HighQuantityScore;

        return Math.Min(riskScore, RiskScoreConstants.MaxRiskScore);
    }

}

/// <summary>
/// ⚠️ DEPRECATED: Bu service layer artık kullanılmıyor.
/// Tüm işlevsellik MediatR handlers'a taşınmıştır (CQRS pattern).
/// Bu dosya sadece referans amaçlı tutulmaktadır ve gelecekte silinebilir.
/// Yeni kod için Merge.Application.Security.Commands ve Merge.Application.Security.Queries kullanın.
/// </summary>
[Obsolete("This service is deprecated. Use MediatR commands and queries instead. See Merge.Application.Security.Commands and Merge.Application.Security.Queries.")]
public class PaymentFraudPreventionService(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<PaymentFraudPreventionService> logger) : IPaymentFraudPreventionService
{

    public async Task<PaymentFraudPreventionDto> CheckPaymentAsync(CreatePaymentFraudCheckDto dto, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Payment fraud check yapılıyor. PaymentId: {PaymentId}, CheckType: {CheckType}",
            dto.PaymentId, dto.CheckType);

        var payment = await context.Set<PaymentEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == dto.PaymentId, cancellationToken);

        if (payment is null)
        {
            throw new NotFoundException("Ödeme", Guid.Empty);
        }

        // Check if check already exists
        var existing = await context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.PaymentId == dto.PaymentId, cancellationToken);

        if (existing is not null)
        {
            existing = await context.Set<PaymentFraudPrevention>()
                .AsNoTracking()
                .Include(c => c.Payment)
                .FirstOrDefaultAsync(c => c.Id == existing.Id, cancellationToken);
            return mapper.Map<PaymentFraudPreventionDto>(existing!);
        }

        // Perform fraud checks
        var riskScore = await PerformFraudChecksAsync(dto, cancellationToken);
        var isBlocked = riskScore >= 70;
        var status = isBlocked ? VerificationStatus.Failed : (riskScore >= 50 ? VerificationStatus.Failed : VerificationStatus.Verified);

        var checkType = Enum.TryParse<PaymentCheckType>(dto.CheckType, true, out var parsedCheckType)
            ? parsedCheckType
            : PaymentCheckType.Device;

        var check = PaymentFraudPrevention.Create(
            paymentId: dto.PaymentId,
            checkType: checkType,
            riskScore: riskScore,
            status: status,
            isBlocked: isBlocked,
            blockReason: isBlocked ? $"High risk score: {riskScore}" : null,
            checkResult: JsonSerializer.Serialize(new { RiskScore = riskScore, CheckType = dto.CheckType }),
            deviceFingerprint: dto.DeviceFingerprint,
            ipAddress: dto.IpAddress,
            userAgent: dto.UserAgent);

        await context.Set<PaymentFraudPrevention>().AddAsync(check, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        check = await context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .Include(c => c.Payment)
            .FirstOrDefaultAsync(c => c.Id == check.Id, cancellationToken);

        logger.LogInformation("Payment fraud check tamamlandı. CheckId: {CheckId}, PaymentId: {PaymentId}, RiskScore: {RiskScore}, IsBlocked: {IsBlocked}",
            check!.Id, dto.PaymentId, riskScore, isBlocked);

        return mapper.Map<PaymentFraudPreventionDto>(check);
    }

    public async Task<PaymentFraudPreventionDto?> GetCheckByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        var check = await context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .Include(c => c.Payment)
            .FirstOrDefaultAsync(c => c.PaymentId == paymentId, cancellationToken);

        if (check is null) return null;

        return mapper.Map<PaymentFraudPreventionDto>(check);
    }

    public async Task<IEnumerable<PaymentFraudPreventionDto>> GetBlockedPaymentsAsync(CancellationToken cancellationToken = default)
    {
        var checks = await context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .Include(c => c.Payment)
            .Where(c => c.IsBlocked)
            .OrderByDescending(c => c.RiskScore)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<PaymentFraudPreventionDto>>(checks);
    }

    public async Task<bool> BlockPaymentAsync(Guid checkId, string reason, CancellationToken cancellationToken = default)
    {
        var check = await context.Set<PaymentFraudPrevention>()
            .FirstOrDefaultAsync(c => c.Id == checkId, cancellationToken);

        if (check is null) return false;

        check.Block(reason);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Payment blocked. CheckId: {CheckId}, Reason: {Reason}", checkId, reason);

        return true;
    }

    public async Task<bool> UnblockPaymentAsync(Guid checkId, CancellationToken cancellationToken = default)
    {
        var check = await context.Set<PaymentFraudPrevention>()
            .FirstOrDefaultAsync(c => c.Id == checkId, cancellationToken);

        if (check is null) return false;

        check.Unblock();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Payment unblocked. CheckId: {CheckId}", checkId);

        return true;
    }

    public async Task<PagedResult<PaymentFraudPreventionDto>> GetAllChecksAsync(string? status = null, bool? isBlocked = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        IQueryable<PaymentFraudPrevention> query = context.Set<PaymentFraudPrevention>()
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

        var checkDtos = mapper.Map<IEnumerable<PaymentFraudPreventionDto>>(checks).ToList();

        return new PagedResult<PaymentFraudPreventionDto>
        {
            Items = checkDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private async Task<int> PerformFraudChecksAsync(CreatePaymentFraudCheckDto dto, CancellationToken cancellationToken = default)
    {
        var payment = await context.Set<PaymentEntity>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Order)
                .ThenInclude(o => o.User)
            .FirstOrDefaultAsync(p => p.Id == dto.PaymentId, cancellationToken);

        if (payment is null) return 0;

        int riskScore = 0;

        // High value payment
        if (payment.Amount > RiskScoreConstants.HighValuePaymentThreshold) 
            riskScore += RiskScoreConstants.HighValuePaymentScore;

        // New user
        var daysSinceRegistration = (DateTime.UtcNow - payment.Order.User.CreatedAt).Days;
        if (daysSinceRegistration < RiskScoreConstants.NewUserDaysThreshold) 
            riskScore += RiskScoreConstants.NewUserScore;

        // Multiple payments from same IP in short time
        var recentPayments = await context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .Where(c => c.IpAddress == dto.IpAddress && c.CreatedAt >= DateTime.UtcNow.AddHours(-RiskScoreConstants.RecentPaymentsTimeWindowHours))
            .CountAsync(cancellationToken);

        if (recentPayments > RiskScoreConstants.RecentPaymentsThreshold) 
            riskScore += RiskScoreConstants.RecentPaymentsScore;

        // Device fingerprint check
        if (string.IsNullOrEmpty(dto.DeviceFingerprint)) 
            riskScore += RiskScoreConstants.MissingDeviceFingerprintScore;

        return Math.Min(riskScore, RiskScoreConstants.MaxRiskScore);
    }

}

/// <summary>
/// ⚠️ DEPRECATED: Bu service layer artık kullanılmıyor.
/// Tüm işlevsellik MediatR handlers'a taşınmıştır (CQRS pattern).
/// Bu dosya sadece referans amaçlı tutulmaktadır ve gelecekte silinebilir.
/// Yeni kod için Merge.Application.Security.Commands ve Merge.Application.Security.Queries kullanın.
/// </summary>
[Obsolete("This service is deprecated. Use MediatR commands and queries instead. See Merge.Application.Security.Commands and Merge.Application.Security.Queries.")]
public class AccountSecurityMonitoringService(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<AccountSecurityMonitoringService> logger,
    IOptions<ServiceSettings> serviceSettings) : IAccountSecurityMonitoringService
{
    private readonly ServiceSettings _serviceSettings = serviceSettings.Value;

    public async Task<AccountSecurityEventDto> LogSecurityEventAsync(CreateAccountSecurityEventDto dto, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Security event loglanıyor. UserId: {UserId}, EventType: {EventType}, Severity: {Severity}",
            dto.UserId, dto.EventType, dto.Severity);

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == dto.UserId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("Kullanıcı", Guid.Empty);
        }

        var eventType = Enum.TryParse<SecurityEventType>(dto.EventType, true, out var parsedEventType)
            ? parsedEventType
            : SecurityEventType.SuspiciousActivity;

        var severity = Enum.TryParse<SecurityEventSeverity>(dto.Severity, true, out var parsedSeverity)
            ? parsedSeverity
            : SecurityEventSeverity.Info;

        var securityEvent = AccountSecurityEvent.Create(
            userId: dto.UserId,
            eventType: eventType,
            severity: severity,
            ipAddress: dto.IpAddress,
            userAgent: dto.UserAgent,
            location: dto.Location,
            deviceFingerprint: dto.DeviceFingerprint,
            isSuspicious: dto.IsSuspicious,
            details: dto.Details is not null ? JsonSerializer.Serialize(dto.Details) : null,
            requiresAction: dto.RequiresAction);

        await context.Set<AccountSecurityEvent>().AddAsync(securityEvent, cancellationToken);

        // If suspicious, create alert
        if (dto.IsSuspicious || dto.RequiresAction)
        {
            var alertSeverity = Enum.TryParse<AlertSeverity>(dto.Severity, true, out var parsedAlertSeverity) 
                ? parsedAlertSeverity 
                : (dto.Severity == "Critical" ? AlertSeverity.Critical : AlertSeverity.High);
            
            var alert = SecurityAlert.Create(
                alertType: AlertType.Account,
                title: $"Suspicious activity detected: {dto.EventType}",
                description: $"Security event: {dto.EventType} for user {user.Email}",
                severity: alertSeverity,
                userId: dto.UserId,
                metadata: dto.Details is not null ? JsonSerializer.Serialize(dto.Details) : null
            );
            await context.Set<SecurityAlert>().AddAsync(alert, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        securityEvent = await context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(e => e.User)
            .Include(e => e.ActionTakenBy)
            .FirstOrDefaultAsync(e => e.Id == securityEvent.Id, cancellationToken);

        logger.LogInformation("Security event loglandı. EventId: {EventId}, UserId: {UserId}, EventType: {EventType}",
            securityEvent!.Id, dto.UserId, dto.EventType);

        return mapper.Map<AccountSecurityEventDto>(securityEvent);
    }

    public async Task<PagedResult<AccountSecurityEventDto>> GetUserSecurityEventsAsync(Guid userId, string? eventType = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var query = context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(e => e.User)
            .Include(e => e.ActionTakenBy)
            .Where(e => e.UserId == userId);

        if (!string.IsNullOrEmpty(eventType))
        {
            if (Enum.TryParse<SecurityEventType>(eventType, true, out var eventTypeEnum))
            {
                query = query.Where(e => e.EventType == eventTypeEnum);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var events = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var eventDtos = mapper.Map<IEnumerable<AccountSecurityEventDto>>(events).ToList();

        return new PagedResult<AccountSecurityEventDto>
        {
            Items = eventDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<AccountSecurityEventDto>> GetSuspiciousEventsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var query = context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(e => e.User)
            .Include(e => e.ActionTakenBy)
            .Where(e => e.IsSuspicious);

        var totalCount = await query.CountAsync(cancellationToken);

        var events = await query
            .OrderByDescending(e => e.Severity == SecurityEventSeverity.Critical)
            .ThenByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var eventDtos = mapper.Map<IEnumerable<AccountSecurityEventDto>>(events).ToList();

        return new PagedResult<AccountSecurityEventDto>
        {
            Items = eventDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> TakeActionAsync(Guid eventId, Guid actionTakenByUserId, string action, string? notes = null, CancellationToken cancellationToken = default)
    {
        var securityEvent = await context.Set<AccountSecurityEvent>()
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

        if (securityEvent is null) return false;

        securityEvent.TakeAction(actionTakenByUserId, action, notes);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Security event action alındı. EventId: {EventId}, Action: {Action}, ActionTakenByUserId: {ActionTakenByUserId}",
            eventId, action, actionTakenByUserId);

        return true;
    }

    public async Task<SecurityAlertDto> CreateSecurityAlertAsync(CreateSecurityAlertDto dto, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Security alert oluşturuluyor. UserId: {UserId}, AlertType: {AlertType}, Severity: {Severity}",
            dto.UserId, dto.AlertType, dto.Severity);

        var severity = Enum.TryParse<AlertSeverity>(dto.Severity, true, out var parsedSeverity) 
            ? parsedSeverity 
            : AlertSeverity.Medium;
        
        // Parse AlertType from string to enum
        if (!Enum.TryParse<AlertType>(dto.AlertType, true, out var alertType))
        {
            logger.LogWarning("Invalid AlertType: {AlertType}, defaulting to Other", dto.AlertType);
            alertType = AlertType.Other;
        }
        
        var alert = SecurityAlert.Create(
            alertType: alertType,
            title: dto.Title,
            description: dto.Description,
            severity: severity,
            userId: dto.UserId,
            metadata: dto.Metadata is not null ? JsonSerializer.Serialize(dto.Metadata) : null
        );

        await context.Set<SecurityAlert>().AddAsync(alert, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        alert = await context.Set<SecurityAlert>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.User)
            .Include(a => a.AcknowledgedBy)
            .Include(a => a.ResolvedBy)
            .FirstOrDefaultAsync(a => a.Id == alert.Id, cancellationToken);

        logger.LogInformation("Security alert oluşturuldu. AlertId: {AlertId}, UserId: {UserId}", alert!.Id, dto.UserId);

        return mapper.Map<SecurityAlertDto>(alert);
    }

    public async Task<PagedResult<SecurityAlertDto>> GetSecurityAlertsAsync(Guid? userId = null, string? severity = null, string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        IQueryable<SecurityAlert> query = context.Set<SecurityAlert>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.User)
            .Include(a => a.AcknowledgedBy)
            .Include(a => a.ResolvedBy);

        if (userId.HasValue)
        {
            query = query.Where(a => a.UserId == userId.Value);
        }

        if (!string.IsNullOrEmpty(severity))
        {
            if (Enum.TryParse<AlertSeverity>(severity, true, out var severityEnum))
            {
                query = query.Where(a => a.Severity == severityEnum);
            }
        }

        if (!string.IsNullOrEmpty(status))
        {
            var statusEnum = Enum.Parse<AlertStatus>(status);
            query = query.Where(a => a.Status == statusEnum);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var alerts = await query
            .OrderByDescending(a => a.Severity == AlertSeverity.Critical)
            .ThenByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var alertDtos = mapper.Map<IEnumerable<SecurityAlertDto>>(alerts).ToList();

        return new PagedResult<SecurityAlertDto>
        {
            Items = alertDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> AcknowledgeAlertAsync(Guid alertId, Guid acknowledgedByUserId, CancellationToken cancellationToken = default)
    {
        var alert = await context.Set<SecurityAlert>()
            .FirstOrDefaultAsync(a => a.Id == alertId, cancellationToken);

        if (alert is null) return false;

        alert.Acknowledge(acknowledgedByUserId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Security alert acknowledged. AlertId: {AlertId}, AcknowledgedByUserId: {AcknowledgedByUserId}",
            alertId, acknowledgedByUserId);

        return true;
    }

    public async Task<bool> ResolveAlertAsync(Guid alertId, Guid resolvedByUserId, string? resolutionNotes = null, CancellationToken cancellationToken = default)
    {
        var alert = await context.Set<SecurityAlert>()
            .FirstOrDefaultAsync(a => a.Id == alertId, cancellationToken);

        if (alert is null) return false;

        alert.Resolve(resolvedByUserId, resolutionNotes);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Security alert resolved. AlertId: {AlertId}, ResolvedByUserId: {ResolvedByUserId}",
            alertId, resolvedByUserId);

        return true;
    }

    public async Task<SecurityMonitoringSummaryDto> GetSecuritySummaryAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-_serviceSettings.DefaultDateRangeDays); // ✅ BOLUM 12.0: Magic number config'den
        var end = endDate ?? DateTime.UtcNow;

        var totalEvents = await context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .Where(e => e.CreatedAt >= start && e.CreatedAt <= end)
            .CountAsync(cancellationToken);

        var suspiciousEvents = await context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .Where(e => e.CreatedAt >= start && e.CreatedAt <= end && e.IsSuspicious)
            .CountAsync(cancellationToken);

        var criticalEvents = await context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .Where(e => e.CreatedAt >= start && e.CreatedAt <= end && e.Severity == SecurityEventSeverity.Critical)
            .CountAsync(cancellationToken);

        var pendingAlerts = await context.Set<SecurityAlert>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end && a.Status == AlertStatus.New)
            .CountAsync(cancellationToken);

        var resolvedAlerts = await context.Set<SecurityAlert>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end && a.Status == AlertStatus.Resolved)
            .CountAsync(cancellationToken);

        var eventsByType = await context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .Where(e => e.CreatedAt >= start && e.CreatedAt <= end)
            .GroupBy(e => e.EventType)
            .Select(g => new { EventType = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.EventType.ToString(), x => x.Count, cancellationToken);

        var alertsBySeverity = await context.Set<SecurityAlert>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end)
            .GroupBy(a => a.Severity)
            .Select(g => new { Severity = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Severity.ToString(), x => x.Count, cancellationToken);

        var recentCriticalAlerts = await context.Set<SecurityAlert>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.User)
            .Include(a => a.AcknowledgedBy)
            .Include(a => a.ResolvedBy)
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end && 
                       a.Severity == AlertSeverity.Critical && a.Status != AlertStatus.Resolved)
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        var recentCriticalAlertsDtos = mapper.Map<IEnumerable<SecurityAlertDto>>(recentCriticalAlerts).ToList();

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

