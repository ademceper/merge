using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.ML;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;


namespace Merge.API.Controllers.ML;

[ApiController]
[Route("api/ml/fraud-detection")]
public class FraudDetectionController : BaseController
{
    private readonly IFraudDetectionService _fraudDetectionService;

    public FraudDetectionController(IFraudDetectionService fraudDetectionService)
    {
        _fraudDetectionService = fraudDetectionService;
    }

    [HttpPost("rules")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<FraudDetectionRuleDto>> CreateRule([FromBody] CreateFraudDetectionRuleDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var rule = await _fraudDetectionService.CreateRuleAsync(dto);
        return CreatedAtAction(nameof(GetRuleById), new { id = rule.Id }, rule);
    }

    [HttpGet("rules/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<FraudDetectionRuleDto>> GetRuleById(Guid id)
    {
        var rule = await _fraudDetectionService.GetRuleByIdAsync(id);
        if (rule == null)
        {
            return NotFound();
        }
        return Ok(rule);
    }

    [HttpGet("rules")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<FraudDetectionRuleDto>>> GetAllRules([FromQuery] string? ruleType = null, [FromQuery] bool? isActive = null)
    {
        var rules = await _fraudDetectionService.GetAllRulesAsync(ruleType, isActive);
        return Ok(rules);
    }

    [HttpPut("rules/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateRule(Guid id, [FromBody] CreateFraudDetectionRuleDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var result = await _fraudDetectionService.UpdateRuleAsync(id, dto);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("rules/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteRule(Guid id)
    {
        var result = await _fraudDetectionService.DeleteRuleAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("evaluate/order/{orderId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<FraudAlertDto>> EvaluateOrder(Guid orderId)
    {
        var alert = await _fraudDetectionService.EvaluateOrderAsync(orderId);
        return Ok(alert);
    }

    [HttpPost("evaluate/payment/{paymentId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<FraudAlertDto>> EvaluatePayment(Guid paymentId)
    {
        var alert = await _fraudDetectionService.EvaluatePaymentAsync(paymentId);
        return Ok(alert);
    }

    [HttpPost("evaluate/user/{userId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<FraudAlertDto>> EvaluateUser(Guid userId)
    {
        var alert = await _fraudDetectionService.EvaluateUserAsync(userId);
        return Ok(alert);
    }

    [HttpGet("alerts")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<FraudAlertDto>>> GetAlerts([FromQuery] string? status = null, [FromQuery] string? alertType = null, [FromQuery] int? minRiskScore = null)
    {
        var alerts = await _fraudDetectionService.GetAlertsAsync(status, alertType, minRiskScore);
        return Ok(alerts);
    }

    [HttpPost("alerts/{alertId}/review")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ReviewAlert(Guid alertId, [FromBody] ReviewAlertDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var reviewedByUserId = GetUserId();
        var result = await _fraudDetectionService.ReviewAlertAsync(alertId, reviewedByUserId, dto.Status, dto.Notes);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpGet("analytics")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<FraudAnalyticsDto>> GetAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var analytics = await _fraudDetectionService.GetFraudAnalyticsAsync(startDate, endDate);
        return Ok(analytics);
    }
}

