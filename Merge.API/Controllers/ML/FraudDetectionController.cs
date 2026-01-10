using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;
using Merge.Application.ML.Commands.CreateFraudDetectionRule;
using Merge.Application.ML.Commands.UpdateFraudDetectionRule;
using Merge.Application.ML.Commands.DeleteFraudDetectionRule;
using Merge.Application.ML.Commands.EvaluateOrder;
using Merge.Application.ML.Commands.EvaluatePayment;
using Merge.Application.ML.Commands.EvaluateUser;
using Merge.Application.ML.Commands.ReviewFraudAlert;
using Merge.Application.ML.Queries.GetFraudDetectionRuleById;
using Merge.Application.ML.Queries.GetAllFraudDetectionRules;
using Merge.Application.ML.Queries.GetFraudAlerts;
using Merge.Application.ML.Queries.GetFraudAnalytics;
using Merge.API.Middleware;

namespace Merge.API.Controllers.ML;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/ml/fraud-detection")]
[Authorize]
public class FraudDetectionController : BaseController
{
    private readonly IMediator _mediator;

    public FraudDetectionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Yeni fraud detection rule oluşturur (Admin, Manager)
    /// </summary>
    /// <param name="dto">Fraud detection rule oluşturma verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan fraud detection rule</returns>
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new CreateFraudDetectionRuleCommand(
            dto.Name,
            dto.RuleType,
            dto.Conditions,
            dto.RiskScore,
            dto.Action,
            dto.IsActive,
            dto.Priority,
            dto.Description);
        var rule = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetRuleById), new { id = rule.Id }, rule);
    }

    /// <summary>
    /// Fraud detection rule detaylarını getirir (Admin, Manager)
    /// </summary>
    /// <param name="id">Fraud detection rule ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Fraud detection rule detayları</returns>
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetFraudDetectionRuleByIdQuery(id);
        var rule = await _mediator.Send(query, cancellationToken);
        if (rule == null)
        {
            return NotFound();
        }
        return Ok(rule);
    }

    /// <summary>
    /// Tüm fraud detection rule'ları getirir (pagination ile) (Admin, Manager)
    /// </summary>
    /// <param name="ruleType">Rule tipi filtresi (opsiyonel)</param>
    /// <param name="isActive">Aktif durum filtresi (opsiyonel)</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış fraud detection rule listesi</returns>
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetAllFraudDetectionRulesQuery(ruleType, isActive, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Fraud detection rule günceller (Admin, Manager)
    /// </summary>
    /// <param name="id">Fraud detection rule ID</param>
    /// <param name="dto">Güncelleme verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Güncelleme sonucu</returns>
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new UpdateFraudDetectionRuleCommand(
            id,
            dto.Name,
            dto.RuleType,
            dto.Conditions,
            dto.RiskScore,
            dto.Action,
            dto.IsActive,
            dto.Priority,
            dto.Description);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Fraud detection rule siler (Admin, Manager)
    /// </summary>
    /// <param name="id">Fraud detection rule ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Silme sonucu</returns>
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new DeleteFraudDetectionRuleCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Sipariş için fraud değerlendirmesi yapar (Admin, Manager)
    /// </summary>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Fraud alert sonucu</returns>
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new EvaluateOrderCommand(orderId);
        var alert = await _mediator.Send(command, cancellationToken);
        return Ok(alert);
    }

    /// <summary>
    /// Ödeme için fraud değerlendirmesi yapar (Admin, Manager)
    /// </summary>
    /// <param name="paymentId">Ödeme ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Fraud alert sonucu</returns>
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new EvaluatePaymentCommand(paymentId);
        var alert = await _mediator.Send(command, cancellationToken);
        return Ok(alert);
    }

    /// <summary>
    /// Kullanıcı için fraud değerlendirmesi yapar (Admin, Manager)
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Fraud alert sonucu</returns>
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new EvaluateUserCommand(userId);
        var alert = await _mediator.Send(command, cancellationToken);
        return Ok(alert);
    }

    /// <summary>
    /// Fraud alert'leri getirir (pagination ile) (Admin, Manager)
    /// </summary>
    /// <param name="status">Alert durumu filtresi (opsiyonel)</param>
    /// <param name="alertType">Alert tipi filtresi (opsiyonel)</param>
    /// <param name="minRiskScore">Minimum risk score filtresi (opsiyonel)</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış fraud alert listesi</returns>
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetFraudAlertsQuery(status, alertType, minRiskScore, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Fraud alert'i gözden geçirir (Admin, Manager)
    /// </summary>
    /// <param name="alertId">Fraud alert ID</param>
    /// <param name="dto">Gözden geçirme verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Gözden geçirme sonucu</returns>
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
        var reviewedByUserId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new ReviewFraudAlertCommand(alertId, reviewedByUserId, dto.Status, dto.Notes);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Fraud analytics verilerini getirir (Admin, Manager)
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
    /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Fraud analytics verileri</returns>
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetFraudAnalyticsQuery(startDate, endDate);
        var analytics = await _mediator.Send(query, cancellationToken);
        return Ok(analytics);
    }
}
