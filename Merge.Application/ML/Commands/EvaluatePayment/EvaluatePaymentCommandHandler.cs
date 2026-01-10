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
using PaymentEntity = Merge.Domain.Entities.Payment;

namespace Merge.Application.ML.Commands.EvaluatePayment;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class EvaluatePaymentCommandHandler : IRequestHandler<EvaluatePaymentCommand, FraudAlertDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<EvaluatePaymentCommandHandler> _logger;
    private readonly FraudDetectionHelper _helper;

    public EvaluatePaymentCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<EvaluatePaymentCommandHandler> logger,
        FraudDetectionHelper helper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _helper = helper;
    }

    public async Task<FraudAlertDto> Handle(EvaluatePaymentCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Evaluating payment for fraud. PaymentId: {PaymentId}", request.PaymentId);

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var payment = await _context.Set<PaymentEntity>()
            .Include(p => p.Order)
                .ThenInclude(o => o.User)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment == null)
        {
            _logger.LogWarning("Payment not found. PaymentId: {PaymentId}", request.PaymentId);
            throw new NotFoundException("Ödeme", request.PaymentId);
        }

        // ✅ BOLUM 1.2: Enum kullanımı (string YASAK)
        var riskScore = await _helper.CalculateRiskScoreAsync(FraudRuleType.Payment, request.PaymentId, payment.Order?.UserId, cancellationToken);
        var matchedRules = await _helper.GetMatchedRulesAsync(FraudRuleType.Payment, request.PaymentId, payment.Order?.UserId, cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var matchedRulesJson = matchedRules.Any() ? JsonSerializer.Serialize(matchedRules.Select(r => r.Id)) : null;
        var alert = FraudAlert.Create(
            userId: payment.Order?.UserId,
            alertType: FraudAlertType.Payment,
            riskScore: riskScore,
            reason: $"Payment evaluation: Risk score {riskScore}",
            paymentId: request.PaymentId,
            matchedRules: matchedRulesJson);

        await _context.Set<FraudAlert>().AddAsync(alert, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with includes in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var createdAlert = await _context.Set<FraudAlert>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Order)
            .Include(a => a.Payment)
            .Include(a => a.ReviewedBy)
            .FirstOrDefaultAsync(a => a.Id == alert.Id, cancellationToken);

        _logger.LogInformation("Payment evaluated. PaymentId: {PaymentId}, AlertId: {AlertId}, RiskScore: {RiskScore}",
            request.PaymentId, alert.Id, alert.RiskScore);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<FraudAlertDto>(createdAlert!);
    }
}
