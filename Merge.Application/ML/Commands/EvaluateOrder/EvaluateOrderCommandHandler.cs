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
using OrderEntity = Merge.Domain.Entities.Order;

namespace Merge.Application.ML.Commands.EvaluateOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class EvaluateOrderCommandHandler : IRequestHandler<EvaluateOrderCommand, FraudAlertDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<EvaluateOrderCommandHandler> _logger;
    private readonly FraudDetectionHelper _helper;

    public EvaluateOrderCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<EvaluateOrderCommandHandler> logger,
        FraudDetectionHelper helper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _helper = helper;
    }

    public async Task<FraudAlertDto> Handle(EvaluateOrderCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Evaluating order for fraud. OrderId: {OrderId}", request.OrderId);

        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        var order = await _context.Set<OrderEntity>()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order not found. OrderId: {OrderId}", request.OrderId);
            throw new NotFoundException("Sipariş", request.OrderId);
        }

        // ✅ BOLUM 1.2: Enum kullanımı (string YASAK)
        var riskScore = await _helper.CalculateRiskScoreAsync(FraudRuleType.Order, request.OrderId, order.UserId, cancellationToken);
        var matchedRules = await _helper.GetMatchedRulesAsync(FraudRuleType.Order, request.OrderId, order.UserId, cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var matchedRulesJson = matchedRules.Any() ? JsonSerializer.Serialize(matchedRules.Select(r => r.Id)) : null;
        var alert = FraudAlert.Create(
            userId: order.UserId,
            alertType: FraudAlertType.Order,
            riskScore: riskScore,
            reason: $"Order evaluation: Risk score {riskScore}",
            orderId: request.OrderId,
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

        _logger.LogInformation("Order evaluated. OrderId: {OrderId}, AlertId: {AlertId}, RiskScore: {RiskScore}",
            request.OrderId, alert.Id, alert.RiskScore);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<FraudAlertDto>(createdAlert!);
    }
}
