using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProductEntity = Merge.Domain.Entities.Product;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.ML;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using System.Text.Json;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;


namespace Merge.Application.Services.ML;

public class FraudDetectionService : IFraudDetectionService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<FraudDetectionService> _logger;

    public FraudDetectionService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<FraudDetectionService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FraudDetectionRuleDto> CreateRuleAsync(CreateFraudDetectionRuleDto dto)
    {
        var rule = new FraudDetectionRule
        {
            Name = dto.Name,
            RuleType = dto.RuleType,
            Conditions = dto.Conditions != null ? JsonSerializer.Serialize(dto.Conditions) : string.Empty,
            RiskScore = dto.RiskScore,
            Action = dto.Action,
            IsActive = dto.IsActive,
            Priority = dto.Priority,
            Description = dto.Description
        };

        await _context.Set<FraudDetectionRule>().AddAsync(rule);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var createdRule = await _context.Set<FraudDetectionRule>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == rule.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<FraudDetectionRuleDto>(createdRule!);
    }

    public async Task<FraudDetectionRuleDto?> GetRuleByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var rule = await _context.Set<FraudDetectionRule>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return rule != null ? _mapper.Map<FraudDetectionRuleDto>(rule) : null;
    }

    public async Task<IEnumerable<FraudDetectionRuleDto>> GetAllRulesAsync(string? ruleType = null, bool? isActive = null)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        IQueryable<FraudDetectionRule> query = _context.Set<FraudDetectionRule>()
            .AsNoTracking();

        if (!string.IsNullOrEmpty(ruleType))
        {
            query = query.Where(r => r.RuleType == ruleType);
        }

        if (isActive.HasValue)
        {
            query = query.Where(r => r.IsActive == isActive.Value);
        }

        var rules = await query
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.Name)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<FraudDetectionRuleDto>>(rules);
    }

    public async Task<bool> UpdateRuleAsync(Guid id, CreateFraudDetectionRuleDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted (Global Query Filter)
        var rule = await _context.Set<FraudDetectionRule>()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (rule == null) return false;

        if (!string.IsNullOrEmpty(dto.Name))
            rule.Name = dto.Name;
        if (!string.IsNullOrEmpty(dto.RuleType))
            rule.RuleType = dto.RuleType;
        if (dto.Conditions != null)
            rule.Conditions = JsonSerializer.Serialize(dto.Conditions);
        rule.RiskScore = dto.RiskScore;
        if (!string.IsNullOrEmpty(dto.Action))
            rule.Action = dto.Action;
        rule.IsActive = dto.IsActive;
        rule.Priority = dto.Priority;
        if (dto.Description != null)
            rule.Description = dto.Description;

        rule.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteRuleAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted (Global Query Filter)
        var rule = await _context.Set<FraudDetectionRule>()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (rule == null) return false;

        rule.IsDeleted = true;
        rule.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<FraudAlertDto> EvaluateOrderAsync(Guid orderId)
    {
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            throw new NotFoundException("Sipariş", orderId);
        }

        var riskScore = await CalculateRiskScoreAsync("Order", orderId, order.UserId);
        var matchedRules = await GetMatchedRulesAsync("Order", orderId, order.UserId);

        var alert = new FraudAlert
        {
            UserId = order.UserId,
            OrderId = orderId,
            AlertType = "Order",
            RiskScore = riskScore,
            Status = FraudAlertStatus.Pending,
            Reason = $"Order evaluation: Risk score {riskScore}",
            // ✅ PERFORMANCE: ToListAsync() sonrası Any() ve Select() YASAK
            // Not: Bu durumda `matchedRules` zaten memory'de (List), bu yüzden bu minimal bir işlem
            // Ancak business logic için gerekli (JSON serialization için)
            MatchedRules = matchedRules.Any() ? JsonSerializer.Serialize(matchedRules.Select(r => r.Id)) : null
        };

        await _context.Set<FraudAlert>().AddAsync(alert);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with includes in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var createdAlert = await _context.Set<FraudAlert>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Order)
            .Include(a => a.Payment)
            .Include(a => a.ReviewedBy)
            .FirstOrDefaultAsync(a => a.Id == alert.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<FraudAlertDto>(createdAlert!);
    }

    public async Task<FraudAlertDto> EvaluatePaymentAsync(Guid paymentId)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var payment = await _context.Payments
            .Include(p => p.Order)
                .ThenInclude(o => o.User)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null)
        {
            throw new NotFoundException("Ödeme", paymentId);
        }

        var riskScore = await CalculateRiskScoreAsync("Payment", paymentId, payment.Order?.UserId);
        var matchedRules = await GetMatchedRulesAsync("Payment", paymentId, payment.Order?.UserId);

        var alert = new FraudAlert
        {
            UserId = payment.Order?.UserId,
            PaymentId = paymentId,
            AlertType = "Payment",
            RiskScore = riskScore,
            Status = FraudAlertStatus.Pending,
            Reason = $"Payment evaluation: Risk score {riskScore}",
            // ✅ PERFORMANCE: ToListAsync() sonrası Any() ve Select() YASAK
            // Not: Bu durumda `matchedRules` zaten memory'de (List), bu yüzden bu minimal bir işlem
            // Ancak business logic için gerekli (JSON serialization için)
            MatchedRules = matchedRules.Any() ? JsonSerializer.Serialize(matchedRules.Select(r => r.Id)) : null
        };

        await _context.Set<FraudAlert>().AddAsync(alert);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with includes in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var createdAlert = await _context.Set<FraudAlert>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Order)
            .Include(a => a.Payment)
            .Include(a => a.ReviewedBy)
            .FirstOrDefaultAsync(a => a.Id == alert.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<FraudAlertDto>(createdAlert!);
    }

    public async Task<FraudAlertDto> EvaluateUserAsync(Guid userId)
    {
        // ✅ PERFORMANCE: Removed manual !u.IsDeleted (Global Query Filter)
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", userId);
        }

        var riskScore = await CalculateRiskScoreAsync("Account", null, userId);
        var matchedRules = await GetMatchedRulesAsync("Account", null, userId);

        var alert = new FraudAlert
        {
            UserId = userId,
            AlertType = "Account",
            RiskScore = riskScore,
            Status = FraudAlertStatus.Pending,
            Reason = $"User evaluation: Risk score {riskScore}",
            // ✅ PERFORMANCE: ToListAsync() sonrası Any() ve Select() YASAK
            // Not: Bu durumda `matchedRules` zaten memory'de (List), bu yüzden bu minimal bir işlem
            // Ancak business logic için gerekli (JSON serialization için)
            MatchedRules = matchedRules.Any() ? JsonSerializer.Serialize(matchedRules.Select(r => r.Id)) : null
        };

        await _context.Set<FraudAlert>().AddAsync(alert);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with includes in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var createdAlert = await _context.Set<FraudAlert>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Order)
            .Include(a => a.Payment)
            .Include(a => a.ReviewedBy)
            .FirstOrDefaultAsync(a => a.Id == alert.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<FraudAlertDto>(createdAlert!);
    }

    public async Task<IEnumerable<FraudAlertDto>> GetAlertsAsync(string? status = null, string? alertType = null, int? minRiskScore = null)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        IQueryable<FraudAlert> query = _context.Set<FraudAlert>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Order)
            .Include(a => a.Payment)
            .Include(a => a.ReviewedBy);

        // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<FraudAlertStatus>(status, true, out var statusEnum))
            {
                query = query.Where(a => a.Status == statusEnum);
            }
        }

        if (!string.IsNullOrEmpty(alertType))
        {
            query = query.Where(a => a.AlertType == alertType);
        }

        if (minRiskScore.HasValue)
        {
            query = query.Where(a => a.RiskScore >= minRiskScore.Value);
        }

        var alerts = await query
            .OrderByDescending(a => a.RiskScore)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<FraudAlertDto>>(alerts);
    }

    public async Task<bool> ReviewAlertAsync(Guid alertId, Guid reviewedByUserId, string status, string? notes = null)
    {
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var alert = await _context.Set<FraudAlert>()
            .FirstOrDefaultAsync(a => a.Id == alertId);

        if (alert == null) return false;

        alert.Status = Enum.Parse<FraudAlertStatus>(status);
        alert.ReviewedByUserId = reviewedByUserId;
        alert.ReviewedAt = DateTime.UtcNow;
        alert.ReviewNotes = notes;
        alert.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<FraudAnalyticsDto> GetFraudAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;

        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var totalAlerts = await _context.Set<FraudAlert>()
            .CountAsync(a => a.CreatedAt >= start && a.CreatedAt <= end);

        var pendingAlerts = await _context.Set<FraudAlert>()
            .CountAsync(a => a.CreatedAt >= start && a.CreatedAt <= end && a.Status == FraudAlertStatus.Pending);

        var resolvedAlerts = await _context.Set<FraudAlert>()
            .CountAsync(a => a.CreatedAt >= start && a.CreatedAt <= end && a.Status == FraudAlertStatus.Resolved);

        var falsePositiveAlerts = await _context.Set<FraudAlert>()
            .CountAsync(a => a.CreatedAt >= start && a.CreatedAt <= end && a.Status == FraudAlertStatus.FalsePositive);

        var avgRiskScore = totalAlerts > 0
            ? await _context.Set<FraudAlert>()
                .Where(a => a.CreatedAt >= start && a.CreatedAt <= end)
                .AverageAsync(a => (decimal?)a.RiskScore) ?? 0
            : 0;

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        var alertsByType = await _context.Set<FraudAlert>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end)
            .GroupBy(a => a.AlertType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count);

        // ✅ BOLUM 1.2: Enum kullanımı - Dictionary için string'e çevir (DTO uyumluluğu için)
        var alertsByStatus = await _context.Set<FraudAlert>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status.ToString(), x => x.Count);

        // ✅ PERFORMANCE: Database'de filtreleme ve sıralama yap (memory'de işlem YASAK)
        var highRiskAlerts = await _context.Set<FraudAlert>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end && a.RiskScore >= 70)
            .OrderByDescending(a => a.RiskScore)
            .Take(10)
            .Select(a => new HighRiskAlertDto
            {
                AlertId = a.Id,
                AlertType = a.AlertType,
                RiskScore = a.RiskScore,
                Status = a.Status.ToString(), // ✅ BOLUM 1.2: Enum -> string (DTO uyumluluğu)
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return new FraudAnalyticsDto
        {
            TotalAlerts = totalAlerts,
            PendingAlerts = pendingAlerts,
            ResolvedAlerts = resolvedAlerts,
            FalsePositiveAlerts = falsePositiveAlerts,
            AverageRiskScore = (decimal)Math.Round(avgRiskScore, 2),
            AlertsByType = alertsByType,
            AlertsByStatus = alertsByStatus,
            HighRiskAlerts = highRiskAlerts
        };
    }

    private async Task<int> CalculateRiskScoreAsync(string alertType, Guid? entityId, Guid? userId)
    {
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted (Global Query Filter)
        var activeRules = await _context.Set<FraudDetectionRule>()
            .Where(r => r.IsActive && r.RuleType == alertType)
            .OrderByDescending(r => r.Priority)
            .ToListAsync();

        int totalRiskScore = 0;

        foreach (var rule in activeRules)
        {
            if (await EvaluateRuleAsync(rule, entityId, userId))
            {
                totalRiskScore += rule.RiskScore;
            }
        }

        return Math.Min(totalRiskScore, 100); // Cap at 100
    }

    private async Task<List<FraudDetectionRule>> GetMatchedRulesAsync(string alertType, Guid? entityId, Guid? userId)
    {
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted (Global Query Filter)
        var activeRules = await _context.Set<FraudDetectionRule>()
            .Where(r => r.IsActive && r.RuleType == alertType)
            .OrderByDescending(r => r.Priority)
            .ToListAsync();

        var matchedRules = new List<FraudDetectionRule>();

        foreach (var rule in activeRules)
        {
            if (await EvaluateRuleAsync(rule, entityId, userId))
            {
                matchedRules.Add(rule);
            }
        }

        return matchedRules;
    }

    private async Task<bool> EvaluateRuleAsync(FraudDetectionRule rule, Guid? entityId, Guid? userId)
    {
        if (string.IsNullOrEmpty(rule.Conditions))
        {
            return false;
        }

        try
        {
            var conditions = JsonSerializer.Deserialize<Dictionary<string, object>>(rule.Conditions);
            if (conditions == null || !conditions.Any())
            {
                return false;
            }

            // Simplified rule evaluation - in production, this would be more sophisticated
            // For now, we'll do basic checks based on common fraud patterns

            if (rule.RuleType == "Order" && entityId.HasValue)
            {
                // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
                var order = await _context.Orders
                    .AsNoTracking()
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == entityId.Value);

                if (order != null)
                {
                    // Example: Check for high-value orders
                    if (conditions.ContainsKey("max_order_amount"))
                    {
                        var maxAmount = Convert.ToDecimal(conditions["max_order_amount"]);
                        if (order.TotalAmount > maxAmount)
                        {
                            return true;
                        }
                    }

                    // Example: Check for multiple items
                    if (conditions.ContainsKey("max_items"))
                    {
                        var maxItems = Convert.ToInt32(conditions["max_items"]);
                        if (order.OrderItems.Count > maxItems)
                        {
                            return true;
                        }
                    }
                }
            }

            if (rule.RuleType == "Payment" && entityId.HasValue)
            {
                // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
                var payment = await _context.Payments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == entityId.Value);

                if (payment != null)
                {
                    // Example: Check for high-value payments
                    if (conditions.ContainsKey("max_payment_amount"))
                    {
                        var maxAmount = Convert.ToDecimal(conditions["max_payment_amount"]);
                        if (payment.Amount > maxAmount)
                        {
                            return true;
                        }
                    }
                }
            }

            if (rule.RuleType == "Account" && userId.HasValue)
            {
                // ✅ PERFORMANCE: Removed manual !u.IsDeleted (Global Query Filter)
                var user = await _context.Users
                    .AsNoTracking()
                    .Include(u => u.Orders)
                    .FirstOrDefaultAsync(u => u.Id == userId.Value);

                if (user != null)
                {
                    // Example: Check for new account with many orders
                    if (conditions.ContainsKey("max_orders_for_new_account"))
                    {
                        var maxOrders = Convert.ToInt32(conditions["max_orders_for_new_account"]);
                        var daysSinceCreation = (DateTime.UtcNow - user.CreatedAt).Days;
                        if (daysSinceCreation < 7 && user.Orders.Count > maxOrders)
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

