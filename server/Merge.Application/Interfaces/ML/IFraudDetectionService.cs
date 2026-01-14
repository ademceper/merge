using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.ML;

public interface IFraudDetectionService
{
    Task<FraudDetectionRuleDto> CreateRuleAsync(CreateFraudDetectionRuleDto dto, CancellationToken cancellationToken = default);
    Task<FraudDetectionRuleDto?> GetRuleByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<FraudDetectionRuleDto>> GetAllRulesAsync(string? ruleType = null, bool? isActive = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateRuleAsync(Guid id, CreateFraudDetectionRuleDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteRuleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FraudAlertDto> EvaluateOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<FraudAlertDto> EvaluatePaymentAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<FraudAlertDto> EvaluateUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FraudAlertDto>> GetAlertsAsync(string? status = null, string? alertType = null, int? minRiskScore = null, CancellationToken cancellationToken = default);
    Task<bool> ReviewAlertAsync(Guid alertId, Guid reviewedByUserId, string status, string? notes = null, CancellationToken cancellationToken = default);
    Task<FraudAnalyticsDto> GetFraudAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}

