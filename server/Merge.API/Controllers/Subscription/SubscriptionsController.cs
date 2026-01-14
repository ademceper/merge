using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Subscription;
using Merge.Application.Subscription.Commands.CreateSubscriptionPlan;
using Merge.Application.Subscription.Commands.UpdateSubscriptionPlan;
using Merge.Application.Subscription.Commands.DeleteSubscriptionPlan;
using Merge.Application.Subscription.Commands.CreateUserSubscription;
using Merge.Application.Subscription.Commands.CancelUserSubscription;
using Merge.Application.Subscription.Commands.RenewSubscription;
using Merge.Application.Subscription.Commands.UpdateUserSubscription;
using Merge.Application.Subscription.Commands.ProcessPayment;
using Merge.Application.Subscription.Commands.RetryFailedPayment;
using Merge.Application.Subscription.Commands.TrackUsage;
using Merge.Application.Subscription.Queries.GetSubscriptionPlanById;
using Merge.Application.Subscription.Queries.GetAllSubscriptionPlans;
using Merge.Application.Subscription.Queries.GetUserSubscriptionById;
using Merge.Application.Subscription.Queries.GetUserActiveSubscription;
using Merge.Application.Subscription.Queries.GetUserSubscriptions;
using Merge.Application.Subscription.Queries.GetSubscriptionPayments;
using Merge.Application.Subscription.Queries.GetAllUsage;
using Merge.Application.Subscription.Queries.GetSubscriptionAnalytics;
using Merge.Application.Subscription.Queries.GetSubscriptionTrends;
using Merge.API.Middleware;
using Merge.Application.Common;
using Merge.Domain.Enums;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
namespace Merge.API.Controllers.Subscription;

[ApiController]
[Route("api/subscriptions")]
[Authorize]
public class SubscriptionsController : BaseController
{
    private readonly IMediator _mediator;

    public SubscriptionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Subscription Plans
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    [HttpGet("plans")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(IEnumerable<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<SubscriptionPlanDto>>> GetAllPlans(
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetAllSubscriptionPlansQuery(isActive);
        var plans = await _mediator.Send(query, cancellationToken);
        return Ok(plans);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    [HttpGet("plans/{id}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(SubscriptionPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SubscriptionPlanDto>> GetPlan(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetSubscriptionPlanByIdQuery(id);
        var plan = await _mediator.Send(query, cancellationToken);
        if (plan == null)
        {
            return NotFound();
        }
        return Ok(plan);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    [HttpPost("plans")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(SubscriptionPlanDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SubscriptionPlanDto>> CreatePlan(
        [FromBody] CreateSubscriptionPlanDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new CreateSubscriptionPlanCommand(
            dto.Name,
            dto.Description,
            dto.PlanType,
            dto.Price,
            dto.DurationDays,
            dto.BillingCycle,
            dto.MaxUsers,
            dto.TrialDays,
            dto.SetupFee,
            dto.Currency ?? "TRY",
            dto.Features,
            dto.IsActive,
            dto.DisplayOrder);

        var plan = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetPlan), new { id = plan.Id }, plan);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    [HttpPut("plans/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdatePlan(
        Guid id,
        [FromBody] UpdateSubscriptionPlanDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new UpdateSubscriptionPlanCommand(
            id,
            dto.Name,
            dto.Description,
            dto.Price,
            dto.DurationDays,
            dto.TrialDays,
            dto.Features,
            dto.IsActive,
            dto.DisplayOrder,
            dto.BillingCycle,
            dto.MaxUsers,
            dto.SetupFee,
            dto.Currency);

        var success = await _mediator.Send(command, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    [HttpDelete("plans/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeletePlan(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new DeleteSubscriptionPlanCommand(id);
        var success = await _mediator.Send(command, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    // User Subscriptions
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU) - spam subscription önleme (5 req/hour)
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    [HttpPost("subscribe")]
    [RateLimit(5, 3600)]
    [ProducesResponseType(typeof(UserSubscriptionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<UserSubscriptionDto>> Subscribe(
        [FromBody] CreateUserSubscriptionDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new CreateUserSubscriptionCommand(userId, dto.SubscriptionPlanId, dto.AutoRenew, dto.PaymentMethodId);
        var subscription = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetMySubscription), new { id = subscription.Id }, subscription);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    [HttpGet("my-subscription")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(UserSubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<UserSubscriptionDto>> GetMySubscription(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var userId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetUserActiveSubscriptionQuery(userId);
        var subscription = await _mediator.Send(query, cancellationToken);
        if (subscription == null)
        {
            return NotFound();
        }
        return Ok(subscription);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    [HttpGet("my-subscriptions")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PagedResult<UserSubscriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<UserSubscriptionDto>>> GetMySubscriptions(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ BOLUM 1.2: Enum kullanımı (string YASAK) - Query string'den enum'a parse
        SubscriptionStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<SubscriptionStatus>(status, true, out var parsedStatus))
        {
            statusEnum = parsedStatus;
        }

        var userId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetUserSubscriptionsQuery(userId, statusEnum, page, pageSize);
        var subscriptions = await _mediator.Send(query, cancellationToken);
        return Ok(subscriptions);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    [HttpPut("subscriptions/{id}")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateSubscription(
        Guid id,
        [FromBody] UpdateUserSubscriptionDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 3.2: IDOR koruması - Kullanıcı sadece kendi subscription'ını güncelleyebilir
        var userId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var getQuery = new GetUserSubscriptionByIdQuery(id);
        var subscription = await _mediator.Send(getQuery, cancellationToken);
        if (subscription == null)
        {
            return NotFound();
        }

        if (subscription.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new UpdateUserSubscriptionCommand(id, dto.AutoRenew, dto.PaymentMethodId);
        var success = await _mediator.Send(command, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    [HttpPost("subscriptions/{id}/cancel")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CancelSubscription(
        Guid id,
        [FromBody] CancelSubscriptionDto? dto = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (dto != null)
        {
            var validationResult = ValidateModelState();
            if (validationResult != null) return validationResult;
        }

        // ✅ BOLUM 3.2: IDOR koruması - Kullanıcı sadece kendi subscription'ını iptal edebilir
        var userId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var getQuery = new GetUserSubscriptionByIdQuery(id);
        var subscription = await _mediator.Send(getQuery, cancellationToken);
        if (subscription == null)
        {
            return NotFound();
        }

        if (subscription.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new CancelUserSubscriptionCommand(id, dto?.Reason);
        var success = await _mediator.Send(command, cancellationToken);
        if (!success)
        {
            return BadRequest("Abonelik iptal edilemedi.");
        }
        return NoContent();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    [HttpPost("subscriptions/{id}/renew")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RenewSubscription(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new RenewSubscriptionCommand(id);
        var success = await _mediator.Send(command, cancellationToken);
        if (!success)
        {
            return BadRequest("Abonelik yenilenemedi.");
        }
        return NoContent();
    }

    // Subscription Payments
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    [HttpGet("subscriptions/{id}/payments")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(IEnumerable<SubscriptionPaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<SubscriptionPaymentDto>>> GetSubscriptionPayments(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.2: IDOR koruması - Kullanıcı sadece kendi subscription'ının payment'larını görebilir
        var userId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var getQuery = new GetUserSubscriptionByIdQuery(id);
        var subscription = await _mediator.Send(getQuery, cancellationToken);
        if (subscription == null)
        {
            return NotFound();
        }

        if (subscription.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var paymentsQuery = new GetSubscriptionPaymentsQuery(id);
        var payments = await _mediator.Send(paymentsQuery, cancellationToken);
        return Ok(payments);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    [HttpPost("payments/{id}/process")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ProcessPayment(
        Guid id,
        [FromBody] ProcessSubscriptionPaymentDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new ProcessPaymentCommand(id, dto.TransactionId);
        var success = await _mediator.Send(command, cancellationToken);
        if (!success)
        {
            return BadRequest("Ödeme işlenemedi.");
        }
        return NoContent();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    [HttpPost("payments/{id}/retry")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RetryPayment(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new RetryFailedPaymentCommand(id);
        var success = await _mediator.Send(command, cancellationToken);
        if (!success)
        {
            return BadRequest("Ödeme tekrar denenemedi.");
        }
        return NoContent();
    }

    // Subscription Usage
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU) - usage tracking abuse önleme (100 req/hour)
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    [HttpPost("usage/track")]
    [RateLimit(100, 3600)]
    [ProducesResponseType(typeof(SubscriptionUsageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SubscriptionUsageDto>> TrackUsage(
        [FromBody] TrackUsageDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var getQuery = new GetUserActiveSubscriptionQuery(userId);
        var subscription = await _mediator.Send(getQuery, cancellationToken);
        if (subscription == null)
        {
            return BadRequest("Aktif abonelik bulunamadı.");
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new TrackUsageCommand(subscription.Id, dto.Feature, dto.Count);
        var usage = await _mediator.Send(command, cancellationToken);
        return Ok(usage);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    [HttpGet("usage")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PagedResult<SubscriptionUsageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<SubscriptionUsageDto>>> GetMyUsage(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var userId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var getQuery = new GetUserActiveSubscriptionQuery(userId);
        var subscription = await _mediator.Send(getQuery, cancellationToken);
        if (subscription == null)
        {
            return BadRequest("Aktif abonelik bulunamadı.");
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var usageQuery = new GetAllUsageQuery(subscription.Id, page, pageSize);
        var usage = await _mediator.Send(usageQuery, cancellationToken);
        return Ok(usage);
    }

    // Analytics
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    [HttpGet("analytics")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(SubscriptionAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SubscriptionAnalyticsDto>> GetAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ VALIDATION: Tarih aralığı kontrolü
        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            return BadRequest("Başlangıç tarihi bitiş tarihinden sonra olamaz.");
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetSubscriptionAnalyticsQuery(startDate, endDate);
        var analytics = await _mediator.Send(query, cancellationToken);
        return Ok(analytics);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    [HttpGet("trends")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(IEnumerable<SubscriptionTrendDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<SubscriptionTrendDto>>> GetTrends(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ VALIDATION: Tarih aralığı kontrolü
        if (startDate > endDate)
        {
            return BadRequest("Başlangıç tarihi bitiş tarihinden sonra olamaz.");
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetSubscriptionTrendsQuery(startDate, endDate);
        var trends = await _mediator.Send(query, cancellationToken);
        return Ok(trends);
    }
}

