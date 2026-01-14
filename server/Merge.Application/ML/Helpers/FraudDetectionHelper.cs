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

// ✅ BOLUM 1.1: Clean Architecture - Helper class for shared fraud detection logic
public class FraudDetectionHelper
{
    private readonly IDbContext _context;
    private readonly ILogger<FraudDetectionHelper> _logger;
    private readonly MLSettings _mlSettings;

    public FraudDetectionHelper(
        IDbContext context,
        ILogger<FraudDetectionHelper> logger,
        IOptions<MLSettings> mlSettings)
    {
        _context = context;
        _logger = logger;
        _mlSettings = mlSettings.Value;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 1.2: Enum kullanımı (string alertType YASAK)
    public async Task<int> CalculateRiskScoreAsync(FraudRuleType ruleType, Guid? entityId, Guid? userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted (Global Query Filter)
        var activeRules = await _context.Set<FraudDetectionRule>()
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

        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        return Math.Min(totalRiskScore, _mlSettings.FraudDetectionRiskScoreCap);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 1.2: Enum kullanımı (string alertType YASAK)
    public async Task<List<FraudDetectionRule>> GetMatchedRulesAsync(FraudRuleType ruleType, Guid? entityId, Guid? userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted (Global Query Filter)
        var activeRules = await _context.Set<FraudDetectionRule>()
            .Where(r => r.IsActive && r.RuleType == ruleType)
            .OrderByDescending(r => r.Priority)
            .ToListAsync(cancellationToken);

        var matchedRules = new List<FraudDetectionRule>();

        foreach (var rule in activeRules)
        {
            if (await EvaluateRuleAsync(rule, entityId, userId, cancellationToken))
            {
                matchedRules.Add(rule);
            }
        }

        return matchedRules;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    private async Task<bool> EvaluateRuleAsync(FraudDetectionRule rule, Guid? entityId, Guid? userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rule.Conditions))
        {
            return false;
        }

        try
        {
            // ✅ BOLUM 4.3: Over-Posting Koruması - Dictionary<string, object> YASAK
            // Typed DTO kullanılıyor
            var conditions = JsonSerializer.Deserialize<FraudRuleConditionsDto>(rule.Conditions);
            if (conditions == null)
            {
                return false;
            }

            // Simplified rule evaluation - in production, this would be more sophisticated
            // For now, we'll do basic checks based on common fraud patterns

            if (rule.RuleType == FraudRuleType.Order && entityId.HasValue)
            {
                // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
                var order = await _context.Set<OrderEntity>()
                    .AsNoTracking()
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == entityId.Value, cancellationToken);

                if (order != null)
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
                // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
                var payment = await _context.Set<PaymentEntity>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == entityId.Value, cancellationToken);

                if (payment != null)
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
                // ✅ PERFORMANCE: Removed manual !u.IsDeleted (Global Query Filter)
                var user = await _context.Users
                    .AsNoTracking()
                    .Include(u => u.Orders)
                    .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

                if (user != null)
                {
                    // Example: Check for new account with many orders
                    if (conditions.NewAccountDays.HasValue)
                    {
                        var daysSinceCreation = (DateTime.UtcNow - user.CreatedAt).Days;
                        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
                        if (daysSinceCreation < _mlSettings.FraudDetectionNewAccountDays && 
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
            _logger.LogError(ex, "Rule evaluation error for rule {RuleId}", rule.Id);
            return false;
        }
    }
}
