using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Exceptions;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;
using Merge.Application.DTOs.ML;
using Merge.Application.Common;
using Merge.Application.ML.Commands.CreateFraudDetectionRule;
using Merge.Application.ML.Commands.UpdateFraudDetectionRule;
using Merge.Application.ML.Commands.PatchFraudDetectionRule;
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

/// <summary>
/// Fraud Detection API endpoints.
/// Dolandırıcılık tespiti işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/ml/fraud-detection")]
[Authorize]
[Tags("FraudDetection")]
public class FraudDetectionController(IMediator mediator) : BaseController
{
    [HttpPost("rules")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(FraudDetectionRuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FraudDetectionRuleDto>> CreateRule(
        [FromBody] CreateFraudDetectionRuleDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateFraudDetectionRuleCommand(
            dto.Name,
            dto.RuleType,
            dto.Conditions,
            dto.RiskScore,
            dto.Action,
            dto.IsActive,
            dto.Priority,
            dto.Description);
        var rule = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetRuleById), new { id = rule.Id }, rule);
    }

    [HttpGet("rules/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(FraudDetectionRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FraudDetectionRuleDto>> GetRuleById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFraudDetectionRuleByIdQuery(id);
        var rule = await mediator.Send(query, cancellationToken)
            ?? throw new NotFoundException("FraudDetectionRule", id);
        return Ok(rule);
    }

    [HttpGet("rules")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
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
        var query = new GetAllFraudDetectionRulesQuery(ruleType, isActive, page, pageSize);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("rules/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)]
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
        var result = await mediator.Send(command, cancellationToken);

        if (!result)
            throw new NotFoundException("FraudDetectionRule", id);

        return NoContent();
    }

    /// <summary>
    /// Fraud detection kuralını kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("rules/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PatchRule(
        Guid id,
        [FromBody] PatchFraudDetectionRuleDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;
        var command = new PatchFraudDetectionRuleCommand(id, patchDto);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
            throw new NotFoundException("FraudDetectionRule", id);

        return NoContent();
    }

    [HttpDelete("rules/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(5, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteRule(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteFraudDetectionRuleCommand(id);
        var result = await mediator.Send(command, cancellationToken);

        if (!result)
            throw new NotFoundException("FraudDetectionRule", id);

        return NoContent();
    }

    [HttpPost("evaluate/order/{orderId}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(FraudAlertDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FraudAlertDto>> EvaluateOrder(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var command = new EvaluateOrderCommand(orderId);
        var alert = await mediator.Send(command, cancellationToken);
        return Ok(alert);
    }

    [HttpPost("evaluate/payment/{paymentId}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(FraudAlertDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FraudAlertDto>> EvaluatePayment(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var command = new EvaluatePaymentCommand(paymentId);
        var alert = await mediator.Send(command, cancellationToken);
        return Ok(alert);
    }

    [HttpPost("evaluate/user/{userId}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(FraudAlertDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FraudAlertDto>> EvaluateUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var command = new EvaluateUserCommand(userId);
        var alert = await mediator.Send(command, cancellationToken);
        return Ok(alert);
    }

    [HttpGet("alerts")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
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
        var query = new GetFraudAlertsQuery(status, alertType, minRiskScore, page, pageSize);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("alerts/{alertId}/review")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
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

        var command = new ReviewFraudAlertCommand(alertId, reviewedByUserId, dto.Status, dto.Notes);
        var result = await mediator.Send(command, cancellationToken);

        if (!result)
            throw new NotFoundException("FraudAlert", alertId);

        return NoContent();
    }

    [HttpGet("analytics")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(FraudAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FraudAnalyticsDto>> GetAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFraudAnalyticsQuery(startDate, endDate);
        var analytics = await mediator.Send(query, cancellationToken);
        return Ok(analytics);
    }
}
