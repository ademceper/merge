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
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.ML.Commands.EvaluateOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class EvaluateOrderCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<EvaluateOrderCommandHandler> logger, FraudDetectionHelper helper) : IRequestHandler<EvaluateOrderCommand, FraudAlertDto>
{

    public async Task<FraudAlertDto> Handle(EvaluateOrderCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Evaluating order for fraud. OrderId: {OrderId}", request.OrderId);

        var order = await context.Set<OrderEntity>()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            logger.LogWarning("Order not found. OrderId: {OrderId}", request.OrderId);
            throw new NotFoundException("Sipariş", request.OrderId);
        }

        // ✅ BOLUM 1.2: Enum kullanımı (string YASAK)
        var riskScore = await helper.CalculateRiskScoreAsync(FraudRuleType.Order, request.OrderId, order.UserId, cancellationToken);
        var matchedRules = await helper.GetMatchedRulesAsync(FraudRuleType.Order, request.OrderId, order.UserId, cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var matchedRulesJson = matchedRules.Any() ? JsonSerializer.Serialize(matchedRules.Select(r => r.Id)) : null;
        var alert = FraudAlert.Create(
            userId: order.UserId,
            alertType: FraudAlertType.Order,
            riskScore: riskScore,
            reason: $"Order evaluation: Risk score {riskScore}",
            orderId: request.OrderId,
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

        logger.LogInformation("Order evaluated. OrderId: {OrderId}, AlertId: {AlertId}, RiskScore: {RiskScore}",
            request.OrderId, alert.Id, alert.RiskScore);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<FraudAlertDto>(createdAlert!);
    }
}
