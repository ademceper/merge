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
using PaymentEntity = Merge.Domain.Modules.Payment.Payment;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Security.Commands.CreatePaymentFraudCheck;

public class CreatePaymentFraudCheckCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreatePaymentFraudCheckCommandHandler> logger, IOptions<SecuritySettings> securitySettings) : IRequestHandler<CreatePaymentFraudCheckCommand, PaymentFraudPreventionDto>
{
    private readonly SecuritySettings securityConfig = securitySettings.Value;

    public async Task<PaymentFraudPreventionDto> Handle(CreatePaymentFraudCheckCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Payment fraud check yapılıyor. PaymentId: {PaymentId}, CheckType: {CheckType}",
            request.PaymentId, request.CheckType);

        var payment = await context.Set<PaymentEntity>()
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment is null)
        {
            throw new NotFoundException("Ödeme", request.PaymentId);
        }

        // Check if check already exists
        var existing = await context.Set<PaymentFraudPrevention>()
            .FirstOrDefaultAsync(c => c.PaymentId == request.PaymentId, cancellationToken);

        if (existing is not null)
        {
            existing = await context.Set<PaymentFraudPrevention>()
                .AsNoTracking()
                .Include(c => c.Payment)
                .FirstOrDefaultAsync(c => c.Id == existing.Id, cancellationToken);
            return mapper.Map<PaymentFraudPreventionDto>(existing!);
        }

        // Perform fraud checks
        var riskScore = await PerformFraudChecksAsync(request, cancellationToken);

        // Parse enum
        var checkType = Enum.TryParse<PaymentCheckType>(request.CheckType, true, out var parsedType)
            ? parsedType
            : throw new BusinessException($"Invalid CheckType: {request.CheckType}");

        var isBlocked = riskScore >= securityConfig.PaymentFraudHighRiskThreshold;
        var status = isBlocked 
            ? VerificationStatus.Failed 
            : (riskScore >= securityConfig.PaymentFraudMediumRiskThreshold 
                ? VerificationStatus.Pending 
                : VerificationStatus.Verified);
        var blockReason = isBlocked ? $"High risk score: {riskScore} (threshold: {securityConfig.PaymentFraudHighRiskThreshold})" : null;

        var checkResultDto = new FraudDetectionMetadataDto
        {
            RiskScore = (decimal)riskScore, // int'ten decimal'e cast
            RiskLevel = riskScore >= securityConfig.PaymentFraudHighRiskThreshold ? "High" :
                       riskScore >= securityConfig.PaymentFraudMediumRiskThreshold ? "Medium" : "Low",
            Decision = riskScore >= securityConfig.PaymentFraudHighRiskThreshold ? "Block" : "Allow",
            DecisionReason = $"Risk score: {riskScore}, Check type: {request.CheckType}"
        };
        var checkResult = JsonSerializer.Serialize(checkResultDto);

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

        await context.Set<PaymentFraudPrevention>().AddAsync(check, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        check = await context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .Include(c => c.Payment)
            .FirstOrDefaultAsync(c => c.Id == check.Id, cancellationToken);

        logger.LogInformation("Payment fraud check tamamlandı. CheckId: {CheckId}, PaymentId: {PaymentId}, RiskScore: {RiskScore}, IsBlocked: {IsBlocked}",
            check!.Id, request.PaymentId, riskScore, check.IsBlocked);

        return mapper.Map<PaymentFraudPreventionDto>(check);
    }

    private async Task<int> PerformFraudChecksAsync(CreatePaymentFraudCheckCommand request, CancellationToken cancellationToken)
    {
        var payment = await context.Set<PaymentEntity>()
            .AsNoTracking()
            .Include(p => p.Order)
                .ThenInclude(o => o.User)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment is null) return 0;

        int riskScore = 0;

        // High value payment - ✅ BOLUM 12.0: Magic number config'den
        if (payment.Amount > securityConfig.HighValuePaymentThreshold) 
            riskScore += securityConfig.HighValuePaymentRiskWeight;

        // New user - ✅ BOLUM 12.0: Magic number config'den
        var daysSinceRegistration = (DateTime.UtcNow - payment.Order.User.CreatedAt).Days;
        if (daysSinceRegistration < securityConfig.NewUserRiskDays) 
            riskScore += securityConfig.NewUserRiskWeight;

        // Multiple payments from same IP in short time - ✅ BOLUM 12.0: Magic number config'den
        var recentPayments = await context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .Where(c => c.IpAddress == request.IpAddress && 
                       c.CreatedAt >= DateTime.UtcNow.AddHours(-securityConfig.RecentPaymentsTimeWindowHours))
            .CountAsync(cancellationToken);

        if (recentPayments > securityConfig.RecentPaymentsFromSameIpThreshold) 
            riskScore += securityConfig.MultiplePaymentsFromSameIpRiskWeight;

        // Device fingerprint check - ✅ BOLUM 12.0: Magic number config'den
        if (string.IsNullOrEmpty(request.DeviceFingerprint)) 
            riskScore += securityConfig.MissingDeviceFingerprintRiskWeight;

        return Math.Min(riskScore, securityConfig.MaxRiskScore);
    }
}
