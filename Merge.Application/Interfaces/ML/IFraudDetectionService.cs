using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Interfaces.ML;

public interface IFraudDetectionService
{
    Task<FraudDetectionRuleDto> CreateRuleAsync(CreateFraudDetectionRuleDto dto);
    Task<FraudDetectionRuleDto?> GetRuleByIdAsync(Guid id);
    Task<IEnumerable<FraudDetectionRuleDto>> GetAllRulesAsync(string? ruleType = null, bool? isActive = null);
    Task<bool> UpdateRuleAsync(Guid id, CreateFraudDetectionRuleDto dto);
    Task<bool> DeleteRuleAsync(Guid id);
    Task<FraudAlertDto> EvaluateOrderAsync(Guid orderId);
    Task<FraudAlertDto> EvaluatePaymentAsync(Guid paymentId);
    Task<FraudAlertDto> EvaluateUserAsync(Guid userId);
    Task<IEnumerable<FraudAlertDto>> GetAlertsAsync(string? status = null, string? alertType = null, int? minRiskScore = null);
    Task<bool> ReviewAlertAsync(Guid alertId, Guid reviewedByUserId, string status, string? notes = null);
    Task<FraudAnalyticsDto> GetFraudAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
}

