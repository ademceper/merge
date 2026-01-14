using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.ML;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using PaymentEntity = Merge.Domain.Modules.Payment.Payment;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;


namespace Merge.Application.Services.ML;

public class FraudDetectionService : IFraudDetectionService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<FraudDetectionService> _logger;
    private readonly MLSettings _mlSettings;

    public FraudDetectionService(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<FraudDetectionService> logger,
        IOptions<MLSettings> mlSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _mlSettings = mlSettings.Value;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<FraudDetectionRuleDto> CreateRuleAsync(CreateFraudDetectionRuleDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Fraud detection rule oluşturuluyor. Name: {Name}, RuleType: {RuleType}, RiskScore: {RiskScore}",
            dto.Name, dto.RuleType, dto.RiskScore);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var ruleType = Enum.TryParse<FraudRuleType>(dto.RuleType, true, out var rt) ? rt : FraudRuleType.Order;
        var action = Enum.TryParse<FraudAction>(dto.Action, true, out var act) ? act : FraudAction.Flag;
        var conditions = dto.Conditions != null ? JsonSerializer.Serialize(dto.Conditions) : string.Empty;
        
        var rule = FraudDetectionRule.Create(
            name: dto.Name,
            ruleType: ruleType,
            conditions: conditions,
            riskScore: dto.RiskScore,
            action: action,
            priority: dto.Priority,
            description: dto.Description);
        
        if (!dto.IsActive)
        {
            rule.Deactivate();
        }

        await _context.Set<FraudDetectionRule>().AddAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var createdRule = await _context.Set<FraudDetectionRule>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == rule.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Fraud detection rule oluşturuldu. RuleId: {RuleId}, Name: {Name}",
            rule.Id, dto.Name);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<FraudDetectionRuleDto>(createdRule!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<FraudDetectionRuleDto?> GetRuleByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var rule = await _context.Set<FraudDetectionRule>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return rule != null ? _mapper.Map<FraudDetectionRuleDto>(rule) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<FraudDetectionRuleDto>> GetAllRulesAsync(string? ruleType = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        IQueryable<FraudDetectionRule> query = _context.Set<FraudDetectionRule>()
            .AsNoTracking();

        if (!string.IsNullOrEmpty(ruleType) && Enum.TryParse<FraudRuleType>(ruleType, true, out var rt))
        {
            query = query.Where(r => r.RuleType == rt);
        }

        if (isActive.HasValue)
        {
            query = query.Where(r => r.IsActive == isActive.Value);
        }

        var rules = await query
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.Name)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<FraudDetectionRuleDto>>(rules);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> UpdateRuleAsync(Guid id, CreateFraudDetectionRuleDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted (Global Query Filter)
        var rule = await _context.Set<FraudDetectionRule>()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rule == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        if (!string.IsNullOrEmpty(dto.Name))
            rule.UpdateName(dto.Name);
        if (dto.Conditions != null)
            rule.UpdateConditions(JsonSerializer.Serialize(dto.Conditions));
        rule.UpdateRiskScore(dto.RiskScore);
        if (!string.IsNullOrEmpty(dto.Action) && Enum.TryParse<FraudAction>(dto.Action, true, out var action))
            rule.UpdateAction(action);
        rule.UpdatePriority(dto.Priority);
        
        if (dto.IsActive && !rule.IsActive)
            rule.Activate();
        else if (!dto.IsActive && rule.IsActive)
            rule.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteRuleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted (Global Query Filter)
        var rule = await _context.Set<FraudDetectionRule>()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rule == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        rule.MarkAsDeleted();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<FraudAlertDto> EvaluateOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        var order = await _context.Set<OrderEntity>()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException("Sipariş", orderId);
        }

        // ✅ BOLUM 1.2: Enum kullanımı (string YASAK)
        var riskScore = await CalculateRiskScoreAsync(FraudRuleType.Order, orderId, order.UserId, cancellationToken);
        var matchedRules = await GetMatchedRulesAsync(FraudRuleType.Order, orderId, order.UserId, cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var matchedRulesJson = matchedRules.Any() ? JsonSerializer.Serialize(matchedRules.Select(r => r.Id)) : null;
        var alert = FraudAlert.Create(
            userId: order.UserId,
            alertType: FraudAlertType.Order,
            riskScore: riskScore,
            reason: $"Order evaluation: Risk score {riskScore}",
            orderId: orderId,
            matchedRules: matchedRulesJson);

        await _context.Set<FraudAlert>().AddAsync(alert, cancellationToken);
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

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<FraudAlertDto>(createdAlert!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<FraudAlertDto> EvaluatePaymentAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var payment = await _context.Set<PaymentEntity>()
            .Include(p => p.Order)
                .ThenInclude(o => o.User)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

        if (payment == null)
        {
            throw new NotFoundException("Ödeme", paymentId);
        }

        // ✅ BOLUM 1.2: Enum kullanımı (string YASAK)
        var riskScore = await CalculateRiskScoreAsync(FraudRuleType.Payment, paymentId, payment.Order?.UserId, cancellationToken);
        var matchedRules = await GetMatchedRulesAsync(FraudRuleType.Payment, paymentId, payment.Order?.UserId, cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var matchedRulesJson = matchedRules.Any() ? JsonSerializer.Serialize(matchedRules.Select(r => r.Id)) : null;
        var alert = FraudAlert.Create(
            userId: payment.Order?.UserId,
            alertType: FraudAlertType.Payment,
            riskScore: riskScore,
            reason: $"Payment evaluation: Risk score {riskScore}",
            paymentId: paymentId,
            matchedRules: matchedRulesJson);

        await _context.Set<FraudAlert>().AddAsync(alert, cancellationToken);
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

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<FraudAlertDto>(createdAlert!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<FraudAlertDto> EvaluateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !u.IsDeleted (Global Query Filter)
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", userId);
        }

        // ✅ BOLUM 1.2: Enum kullanımı (string YASAK)
        var riskScore = await CalculateRiskScoreAsync(FraudRuleType.Account, null, userId, cancellationToken);
        var matchedRules = await GetMatchedRulesAsync(FraudRuleType.Account, null, userId, cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var matchedRulesJson = matchedRules.Any() ? JsonSerializer.Serialize(matchedRules.Select(r => r.Id)) : null;
        var alert = FraudAlert.Create(
            userId: userId,
            alertType: FraudAlertType.Account,
            riskScore: riskScore,
            reason: $"User evaluation: Risk score {riskScore}",
            matchedRules: matchedRulesJson);

        await _context.Set<FraudAlert>().AddAsync(alert, cancellationToken);
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

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<FraudAlertDto>(createdAlert!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<FraudAlertDto>> GetAlertsAsync(string? status = null, string? alertType = null, int? minRiskScore = null, CancellationToken cancellationToken = default)
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

        // ✅ BOLUM 1.2: Enum kullanımı (string AlertType YASAK)
        if (!string.IsNullOrEmpty(alertType))
        {
            if (Enum.TryParse<FraudAlertType>(alertType, true, out var alertTypeEnum))
            {
                query = query.Where(a => a.AlertType == alertTypeEnum);
            }
        }

        if (minRiskScore.HasValue)
        {
            query = query.Where(a => a.RiskScore >= minRiskScore.Value);
        }

        var alerts = await query
            .OrderByDescending(a => a.RiskScore)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<FraudAlertDto>>(alerts);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> ReviewAlertAsync(Guid alertId, Guid reviewedByUserId, string status, string? notes = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var alert = await _context.Set<FraudAlert>()
            .FirstOrDefaultAsync(a => a.Id == alertId, cancellationToken);

        if (alert == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        if (Enum.TryParse<FraudAlertStatus>(status, true, out var statusEnum))
        {
            alert.Review(reviewedByUserId, statusEnum, notes);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<FraudAnalyticsDto> GetFraudAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;

        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var totalAlerts = await _context.Set<FraudAlert>()
            .CountAsync(a => a.CreatedAt >= start && a.CreatedAt <= end, cancellationToken);

        var pendingAlerts = await _context.Set<FraudAlert>()
            .CountAsync(a => a.CreatedAt >= start && a.CreatedAt <= end && a.Status == FraudAlertStatus.Pending, cancellationToken);

        var resolvedAlerts = await _context.Set<FraudAlert>()
            .CountAsync(a => a.CreatedAt >= start && a.CreatedAt <= end && a.Status == FraudAlertStatus.Resolved, cancellationToken);

        var falsePositiveAlerts = await _context.Set<FraudAlert>()
            .CountAsync(a => a.CreatedAt >= start && a.CreatedAt <= end && a.Status == FraudAlertStatus.FalsePositive, cancellationToken);

        var avgRiskScore = totalAlerts > 0
            ? await _context.Set<FraudAlert>()
                .Where(a => a.CreatedAt >= start && a.CreatedAt <= end)
                .AverageAsync(a => (decimal?)a.RiskScore, cancellationToken) ?? 0
            : 0;

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        // ✅ BOLUM 1.2: Enum kullanımı - Dictionary için string'e çevir (DTO uyumluluğu için)
        var alertsByTypeRaw = await _context.Set<FraudAlert>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end)
            .GroupBy(a => a.AlertType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count, cancellationToken);
        
        var alertsByType = alertsByTypeRaw.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);

        // ✅ BOLUM 1.2: Enum kullanımı - Dictionary için string'e çevir (DTO uyumluluğu için)
        var alertsByStatus = await _context.Set<FraudAlert>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status.ToString(), x => x.Count, cancellationToken);

        // ✅ PERFORMANCE: Database'de filtreleme ve sıralama yap (memory'de işlem YASAK)
        var highRiskAlerts = await _context.Set<FraudAlert>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= start && a.CreatedAt <= end && a.RiskScore >= 70)
            .OrderByDescending(a => a.RiskScore)
            .Take(10)
            .Select(a => new HighRiskAlertDto(
                a.Id,
                a.AlertType.ToString(), // ✅ BOLUM 1.2: Enum -> string (DTO uyumluluğu)
                a.RiskScore,
                a.Status.ToString(), // ✅ BOLUM 1.2: Enum -> string (DTO uyumluluğu)
                a.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new FraudAnalyticsDto(
            totalAlerts,
            pendingAlerts,
            resolvedAlerts,
            falsePositiveAlerts,
            (decimal)Math.Round(avgRiskScore, 2),
            alertsByType,
            alertsByStatus,
            highRiskAlerts
        );
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 1.2: Enum kullanımı (string alertType YASAK)
    private async Task<int> CalculateRiskScoreAsync(FraudRuleType ruleType, Guid? entityId, Guid? userId, CancellationToken cancellationToken = default)
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
        // Not: MLSettings.MaxRiskScore kullanılabilir, ancak şu an service'te configuration yok
        // Bu method private olduğu için şimdilik 100 kullanıyoruz
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        return Math.Min(totalRiskScore, _mlSettings.MaxRiskScore);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 1.2: Enum kullanımı (string alertType YASAK)
    private async Task<List<FraudDetectionRule>> GetMatchedRulesAsync(FraudRuleType ruleType, Guid? entityId, Guid? userId, CancellationToken cancellationToken = default)
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
            var conditions = JsonSerializer.Deserialize<Dictionary<string, object>>(rule.Conditions);
            if (conditions == null || !conditions.Any())
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

            if (rule.RuleType == FraudRuleType.Payment && entityId.HasValue)
            {
                // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
                var payment = await _context.Set<PaymentEntity>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == entityId.Value, cancellationToken);

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
                    if (conditions.ContainsKey("max_orders_for_new_account"))
                    {
                        var maxOrders = Convert.ToInt32(conditions["max_orders_for_new_account"]);
                        var daysSinceCreation = (DateTime.UtcNow - user.CreatedAt).Days;
                        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
                        if (daysSinceCreation < _mlSettings.NewAccountCheckDays && user.Orders.Count > maxOrders)
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

