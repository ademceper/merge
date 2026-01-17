using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.ML.Helpers;
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

namespace Merge.Application.ML.Commands.EvaluatePayment;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class EvaluatePaymentCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<EvaluatePaymentCommandHandler> logger, FraudDetectionHelper helper) : IRequestHandler<EvaluatePaymentCommand, FraudAlertDto>
{

    public async Task<FraudAlertDto> Handle(EvaluatePaymentCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Evaluating payment for fraud. PaymentId: {PaymentId}", request.PaymentId);

        var payment = await context.Set<PaymentEntity>()
            .Include(p => p.Order)
                .ThenInclude(o => o.User)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment == null)
        {
            logger.LogWarning("Payment not found. PaymentId: {PaymentId}", request.PaymentId);
            throw new NotFoundException("Ödeme", request.PaymentId);
        }

        // ✅ BOLUM 1.2: Enum kullanımı (string YASAK)
        var riskScore = await helper.CalculateRiskScoreAsync(FraudRuleType.Payment, request.PaymentId, payment.Order?.UserId, cancellationToken);
        var matchedRules = await helper.GetMatchedRulesAsync(FraudRuleType.Payment, request.PaymentId, payment.Order?.UserId, cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var matchedRulesJson = matchedRules.Any() ? JsonSerializer.Serialize(matchedRules.Select(r => r.Id)) : null;
        var alert = FraudAlert.Create(
            userId: payment.Order?.UserId,
            alertType: FraudAlertType.Payment,
            riskScore: riskScore,
            reason: $"Payment evaluation: Risk score {riskScore}",
            paymentId: request.PaymentId,
            matchedRules: matchedRulesJson);

        await context.Set<FraudAlert>().AddAsync(alert, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var createdAlert = await context.Set<FraudAlert>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Order)
            .Include(a => a.Payment)
            .Include(a => a.ReviewedBy)
            .FirstOrDefaultAsync(a => a.Id == alert.Id, cancellationToken);

        logger.LogInformation("Payment evaluated. PaymentId: {PaymentId}, AlertId: {AlertId}, RiskScore: {RiskScore}",
            request.PaymentId, alert.Id, alert.RiskScore);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<FraudAlertDto>(createdAlert!);
    }
}
