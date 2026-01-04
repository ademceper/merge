using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.ML;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;
using Merge.Application.Common;
using Merge.API.Middleware;

namespace Merge.API.Controllers.ML;

[ApiController]
[Route("api/ml/fraud-detection")]
[Authorize]
public class FraudDetectionController : BaseController
{
    private readonly IFraudDetectionService _fraudDetectionService;

    public FraudDetectionController(IFraudDetectionService fraudDetectionService)
    {
        _fraudDetectionService = fraudDetectionService;
    }

    /// <summary>
    /// Yeni fraud detection rule oluşturur (Admin, Manager)
    /// </summary>
    [HttpPost("rules")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(typeof(FraudDetectionRuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FraudDetectionRuleDto>> CreateRule(
        [FromBody] CreateFraudDetectionRuleDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var rule = await _fraudDetectionService.CreateRuleAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetRuleById), new { id = rule.Id }, rule);
    }

    /// <summary>
    /// Fraud detection rule detaylarını getirir (Admin, Manager)
    /// </summary>
    [HttpGet("rules/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(FraudDetectionRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FraudDetectionRuleDto>> GetRuleById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var rule = await _fraudDetectionService.GetRuleByIdAsync(id, cancellationToken);
        if (rule == null)
        {
            return NotFound();
        }
        return Ok(rule);
    }

    /// <summary>
    /// Tüm fraud detection rule'ları getirir (pagination ile) (Admin, Manager)
    /// </summary>
    [HttpGet("rules")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<FraudDetectionRuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<FraudDetectionRuleDto>>> GetAllRules(
        [FromQuery] string? ruleType = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        if (pageSize > 100) pageSize = 100; // Max limit
        if (page < 1) page = 1;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var allRules = await _fraudDetectionService.GetAllRulesAsync(ruleType, isActive, cancellationToken);
        var rulesList = allRules.ToList();

        // ✅ BOLUM 3.4: Pagination implementation
        var totalCount = rulesList.Count;
        var pagedRules = rulesList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = new PagedResult<FraudDetectionRuleDto>
        {
            Items = pagedRules,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return Ok(result);
    }

    /// <summary>
    /// Fraud detection rule günceller (Admin, Manager)
    /// </summary>
    [HttpPut("rules/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateRule(
        Guid id,
        [FromBody] CreateFraudDetectionRuleDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _fraudDetectionService.UpdateRuleAsync(id, dto, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Fraud detection rule siler (Admin, Manager)
    /// </summary>
    [HttpDelete("rules/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteRule(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _fraudDetectionService.DeleteRuleAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Sipariş için fraud değerlendirmesi yapar (Admin, Manager)
    /// </summary>
    [HttpPost("evaluate/order/{orderId}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(FraudAlertDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FraudAlertDto>> EvaluateOrder(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var alert = await _fraudDetectionService.EvaluateOrderAsync(orderId, cancellationToken);
        return Ok(alert);
    }

    /// <summary>
    /// Ödeme için fraud değerlendirmesi yapar (Admin, Manager)
    /// </summary>
    [HttpPost("evaluate/payment/{paymentId}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(FraudAlertDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FraudAlertDto>> EvaluatePayment(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var alert = await _fraudDetectionService.EvaluatePaymentAsync(paymentId, cancellationToken);
        return Ok(alert);
    }

    /// <summary>
    /// Kullanıcı için fraud değerlendirmesi yapar (Admin, Manager)
    /// </summary>
    [HttpPost("evaluate/user/{userId}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(FraudAlertDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FraudAlertDto>> EvaluateUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var alert = await _fraudDetectionService.EvaluateUserAsync(userId, cancellationToken);
        return Ok(alert);
    }

    /// <summary>
    /// Fraud alert'leri getirir (pagination ile) (Admin, Manager)
    /// </summary>
    [HttpGet("alerts")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<FraudAlertDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<FraudAlertDto>>> GetAlerts(
        [FromQuery] string? status = null,
        [FromQuery] string? alertType = null,
        [FromQuery] int? minRiskScore = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        if (pageSize > 100) pageSize = 100; // Max limit
        if (page < 1) page = 1;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var allAlerts = await _fraudDetectionService.GetAlertsAsync(status, alertType, minRiskScore, cancellationToken);
        var alertsList = allAlerts.ToList();

        // ✅ BOLUM 3.4: Pagination implementation
        var totalCount = alertsList.Count;
        var pagedAlerts = alertsList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = new PagedResult<FraudAlertDto>
        {
            Items = pagedAlerts,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return Ok(result);
    }

    /// <summary>
    /// Fraud alert'i gözden geçirir (Admin, Manager)
    /// </summary>
    [HttpPost("alerts/{alertId}/review")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ReviewAlert(
        Guid alertId,
        [FromBody] ReviewAlertDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var reviewedByUserId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _fraudDetectionService.ReviewAlertAsync(alertId, reviewedByUserId, dto.Status, dto.Notes, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Fraud analytics verilerini getirir (Admin, Manager)
    /// </summary>
    [HttpGet("analytics")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(FraudAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FraudAnalyticsDto>> GetAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var analytics = await _fraudDetectionService.GetFraudAnalyticsAsync(startDate, endDate, cancellationToken);
        return Ok(analytics);
    }
}
