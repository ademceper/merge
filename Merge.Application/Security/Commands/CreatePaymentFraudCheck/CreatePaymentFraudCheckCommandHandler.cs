using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Security;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;
using PaymentEntity = Merge.Domain.Entities.Payment;

namespace Merge.Application.Security.Commands.CreatePaymentFraudCheck;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreatePaymentFraudCheckCommandHandler : IRequestHandler<CreatePaymentFraudCheckCommand, PaymentFraudPreventionDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreatePaymentFraudCheckCommandHandler> _logger;
    private readonly SecuritySettings _securitySettings;

    public CreatePaymentFraudCheckCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreatePaymentFraudCheckCommandHandler> logger,
        IOptions<SecuritySettings> securitySettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _securitySettings = securitySettings.Value;
    }

    public async Task<PaymentFraudPreventionDto> Handle(CreatePaymentFraudCheckCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Payment fraud check yapılıyor. PaymentId: {PaymentId}, CheckType: {CheckType}",
            request.PaymentId, request.CheckType);

        var payment = await _context.Set<PaymentEntity>()
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment == null)
        {
            throw new NotFoundException("Ödeme", request.PaymentId);
        }

        // Check if check already exists
        var existing = await _context.Set<PaymentFraudPrevention>()
            .FirstOrDefaultAsync(c => c.PaymentId == request.PaymentId, cancellationToken);

        if (existing != null)
        {
            existing = await _context.Set<PaymentFraudPrevention>()
                .AsNoTracking()
                .Include(c => c.Payment)
                .FirstOrDefaultAsync(c => c.Id == existing.Id, cancellationToken);
            return _mapper.Map<PaymentFraudPreventionDto>(existing!);
        }

        // Perform fraud checks
        var riskScore = await PerformFraudChecksAsync(request, cancellationToken);

        // Parse enum
        var checkType = Enum.TryParse<PaymentCheckType>(request.CheckType, true, out var parsedType)
            ? parsedType
            : throw new BusinessException($"Invalid CheckType: {request.CheckType}");

        // ✅ BOLUM 12.0: Magic number config'den - Risk score'a göre isBlocked ve status belirleme
        var isBlocked = riskScore >= _securitySettings.PaymentFraudHighRiskThreshold;
        var status = isBlocked 
            ? VerificationStatus.Failed 
            : (riskScore >= _securitySettings.PaymentFraudMediumRiskThreshold 
                ? VerificationStatus.Pending 
                : VerificationStatus.Verified);
        var blockReason = isBlocked ? $"High risk score: {riskScore} (threshold: {_securitySettings.PaymentFraudHighRiskThreshold})" : null;

        // ✅ SECURITY: Dictionary<string,object> yerine typed DTO kullaniyoruz
        var checkResultDto = new FraudDetectionMetadataDto
        {
            RiskScore = (decimal)riskScore, // int'ten decimal'e cast
            RiskLevel = riskScore >= _securitySettings.PaymentFraudHighRiskThreshold ? "High" :
                       riskScore >= _securitySettings.PaymentFraudMediumRiskThreshold ? "Medium" : "Low",
            Decision = riskScore >= _securitySettings.PaymentFraudHighRiskThreshold ? "Block" : "Allow",
            DecisionReason = $"Risk score: {riskScore}, Check type: {request.CheckType}"
        };
        var checkResult = JsonSerializer.Serialize(checkResultDto);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        // ✅ BOLUM 12.0: Magic number config'den - Handler'da belirlenen isBlocked ve status Create method'una geçiriliyor
        var check = PaymentFraudPrevention.Create(
            paymentId: request.PaymentId,
            checkType: checkType,
            riskScore: riskScore,
            status: status,
            isBlocked: isBlocked,
            blockReason: blockReason,
            checkResult: checkResult,
            deviceFingerprint: request.DeviceFingerprint,
            ipAddress: request.IpAddress,
            userAgent: request.UserAgent);

        await _context.Set<PaymentFraudPrevention>().AddAsync(check, cancellationToken);
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        check = await _context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .Include(c => c.Payment)
            .FirstOrDefaultAsync(c => c.Id == check.Id, cancellationToken);

        _logger.LogInformation("Payment fraud check tamamlandı. CheckId: {CheckId}, PaymentId: {PaymentId}, RiskScore: {RiskScore}, IsBlocked: {IsBlocked}",
            check!.Id, request.PaymentId, riskScore, check.IsBlocked);

        return _mapper.Map<PaymentFraudPreventionDto>(check);
    }

    private async Task<int> PerformFraudChecksAsync(CreatePaymentFraudCheckCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsSplitQuery - ThenInclude kullanımı için Cartesian Explosion önleme
        var payment = await _context.Set<PaymentEntity>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Order)
                .ThenInclude(o => o.User)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment == null) return 0;

        int riskScore = 0;

        // High value payment - ✅ BOLUM 12.0: Magic number config'den
        if (payment.Amount > _securitySettings.HighValuePaymentThreshold) 
            riskScore += _securitySettings.HighValuePaymentRiskWeight;

        // New user - ✅ BOLUM 12.0: Magic number config'den
        var daysSinceRegistration = (DateTime.UtcNow - payment.Order.User.CreatedAt).Days;
        if (daysSinceRegistration < _securitySettings.NewUserRiskDays) 
            riskScore += _securitySettings.NewUserRiskWeight;

        // Multiple payments from same IP in short time - ✅ BOLUM 12.0: Magic number config'den
        var recentPayments = await _context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .Where(c => c.IpAddress == request.IpAddress && 
                       c.CreatedAt >= DateTime.UtcNow.AddHours(-_securitySettings.RecentPaymentsTimeWindowHours))
            .CountAsync(cancellationToken);

        if (recentPayments > _securitySettings.RecentPaymentsFromSameIpThreshold) 
            riskScore += _securitySettings.MultiplePaymentsFromSameIpRiskWeight;

        // Device fingerprint check - ✅ BOLUM 12.0: Magic number config'den
        if (string.IsNullOrEmpty(request.DeviceFingerprint)) 
            riskScore += _securitySettings.MissingDeviceFingerprintRiskWeight;

        return Math.Min(riskScore, _securitySettings.MaxRiskScore);
    }
}
