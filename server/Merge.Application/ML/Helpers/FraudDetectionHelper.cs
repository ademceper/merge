using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Application.DTOs.Content;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using PaymentEntity = Merge.Domain.Modules.Payment.Payment;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.ML.Helpers;

public class FraudDetectionHelper(IDbContext context, ILogger<FraudDetectionHelper> logger, IOptions<MLSettings> mlSettings)
{
    private readonly MLSettings config = mlSettings.Value;

    public async Task<int> CalculateRiskScoreAsync(FraudRuleType ruleType, Guid? entityId, Guid? userId, CancellationToken cancellationToken = default)
    {
        var activeRules = await context.Set<FraudDetectionRule>()
            .Where(r => r.IsActive && r.RuleType == ruleType)
            .OrderByDescending(r => r.Priority)
            .ToListAsync(cancellationToken);

        int totalRiskScore = 0;

        foreach (var rule in activeRules)
        {
            if (await EvaluateRuleAsync(rule, entityId, userId, cancellationToken))
            {
                totalRiskScore += rule.RiskScore;
            }
        }

        var mlConfig = mlSettings.Value;
        return Math.Min(totalRiskScore, mlConfig.FraudDetectionRiskScoreCap);
    }

    public async Task<List<FraudDetectionRule>> GetMatchedRulesAsync(FraudRuleType ruleType, Guid? entityId, Guid? userId, CancellationToken cancellationToken = default)
    {
        var activeRules = await context.Set<FraudDetectionRule>()
            .Where(r => r.IsActive && r.RuleType == ruleType)
            .OrderByDescending(r => r.Priority)
            .ToListAsync(cancellationToken);

        List<FraudDetectionRule> matchedRules = [];

        foreach (var rule in activeRules)
        {
            if (await EvaluateRuleAsync(rule, entityId, userId, cancellationToken))
            {
                matchedRules.Add(rule);
            }
        }

        return matchedRules;
    }

    private async Task<bool> EvaluateRuleAsync(FraudDetectionRule rule, Guid? entityId, Guid? userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rule.Conditions))
        {
            return false;
        }

        try
        {
            // Typed DTO kullanılıyor
            var conditions = JsonSerializer.Deserialize<FraudRuleConditionsDto>(rule.Conditions);
            if (conditions is null)
            {
                return false;
            }

            // Simplified rule evaluation - in production, this would be more sophisticated
            // For now, we'll do basic checks based on common fraud patterns

            if (rule.RuleType == FraudRuleType.Order && entityId.HasValue)
            {
                var order = await context.Set<OrderEntity>()
                    .AsNoTracking()
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == entityId.Value, cancellationToken);

                if (order is not null)
                {
                    // Example: Check for high-value orders
                    if (conditions.MaxTransactionAmount.HasValue && order.TotalAmount > conditions.MaxTransactionAmount.Value)
                    {
                        return true;
                    }

                    // Example: Check for multiple items (using MaxDailyTransactions as max items per order)
                    if (conditions.MaxDailyTransactions.HasValue && order.OrderItems.Count > conditions.MaxDailyTransactions.Value)
                    {
                        return true;
                    }
                }
            }

            if (rule.RuleType == FraudRuleType.Payment && entityId.HasValue)
            {
                var payment = await context.Set<PaymentEntity>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == entityId.Value, cancellationToken);

                if (payment is not null)
                {
                    // Example: Check for high-value payments
                    if (conditions.MaxTransactionAmount.HasValue && payment.Amount > conditions.MaxTransactionAmount.Value)
                    {
                        return true;
                    }
                }
            }

            if (rule.RuleType == FraudRuleType.Account && userId.HasValue)
            {
                var user = await context.Users
                    .AsNoTracking()
                    .Include(u => u.Orders)
                    .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

                if (user is not null)
                {
                    // Example: Check for new account with many orders
                    if (conditions.NewAccountDays.HasValue)
                    {
                        var daysSinceCreation = (DateTime.UtcNow - user.CreatedAt).Days;
                        if (daysSinceCreation < config.FraudDetectionNewAccountDays && 
                            daysSinceCreation < conditions.NewAccountDays.Value && 
                            user.Orders.Count > (conditions.MaxDailyTransactions ?? int.MaxValue))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Rule evaluation error for rule {RuleId}", rule.Id);
            return false;
        }
    }
}
