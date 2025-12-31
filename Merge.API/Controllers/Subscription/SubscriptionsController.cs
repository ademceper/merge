using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Subscription;
using Merge.Application.DTOs.Subscription;

namespace Merge.API.Controllers.Subscription;

[ApiController]
[Route("api/subscriptions")]
[Authorize]
public class SubscriptionsController : BaseController
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    // Subscription Plans
    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<SubscriptionPlanDto>>> GetAllPlans([FromQuery] bool? isActive = null)
    {
        var plans = await _subscriptionService.GetAllSubscriptionPlansAsync(isActive);
        return Ok(plans);
    }

    [HttpGet("plans/{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<SubscriptionPlanDto>> GetPlan(Guid id)
    {
        var plan = await _subscriptionService.GetSubscriptionPlanByIdAsync(id);
        if (plan == null)
        {
            return NotFound();
        }
        return Ok(plan);
    }

    [HttpPost("plans")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<SubscriptionPlanDto>> CreatePlan([FromBody] CreateSubscriptionPlanDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var plan = await _subscriptionService.CreateSubscriptionPlanAsync(dto);
        return CreatedAtAction(nameof(GetPlan), new { id = plan.Id }, plan);
    }

    [HttpPut("plans/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdatePlan(Guid id, [FromBody] UpdateSubscriptionPlanDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var success = await _subscriptionService.UpdateSubscriptionPlanAsync(id, dto);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("plans/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeletePlan(Guid id)
    {
        var success = await _subscriptionService.DeleteSubscriptionPlanAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    // User Subscriptions
    [HttpPost("subscribe")]
    public async Task<ActionResult<UserSubscriptionDto>> Subscribe([FromBody] CreateUserSubscriptionDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var subscription = await _subscriptionService.CreateUserSubscriptionAsync(userId, dto);
        return CreatedAtAction(nameof(GetMySubscription), new { id = subscription.Id }, subscription);
    }

    [HttpGet("my-subscription")]
    public async Task<ActionResult<UserSubscriptionDto>> GetMySubscription()
    {
        var userId = GetUserId();
        var subscription = await _subscriptionService.GetUserActiveSubscriptionAsync(userId);
        if (subscription == null)
        {
            return NotFound();
        }
        return Ok(subscription);
    }

    [HttpGet("my-subscriptions")]
    public async Task<ActionResult<IEnumerable<UserSubscriptionDto>>> GetMySubscriptions([FromQuery] string? status = null)
    {
        var userId = GetUserId();
        var subscriptions = await _subscriptionService.GetUserSubscriptionsAsync(userId, status);
        return Ok(subscriptions);
    }

    [HttpPut("subscriptions/{id}")]
    public async Task<IActionResult> UpdateSubscription(Guid id, [FromBody] UpdateUserSubscriptionDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ ARCHITECTURE: Authorization check - kullanıcı sadece kendi subscription'ını güncelleyebilir
        var userId = GetUserId();
        var subscription = await _subscriptionService.GetUserSubscriptionByIdAsync(id);
        if (subscription == null)
        {
            return NotFound();
        }

        if (subscription.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var success = await _subscriptionService.UpdateUserSubscriptionAsync(id, dto);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("subscriptions/{id}/cancel")]
    public async Task<IActionResult> CancelSubscription(Guid id, [FromBody] CancelSubscriptionDto? dto = null)
    {
        if (dto != null)
        {
            var validationResult = ValidateModelState();
            if (validationResult != null) return validationResult;
        }

        // ✅ ARCHITECTURE: Authorization check - kullanıcı sadece kendi subscription'ını iptal edebilir
        var userId = GetUserId();
        var subscription = await _subscriptionService.GetUserSubscriptionByIdAsync(id);
        if (subscription == null)
        {
            return NotFound();
        }

        if (subscription.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var success = await _subscriptionService.CancelUserSubscriptionAsync(id, dto?.Reason);
        if (!success)
        {
            return BadRequest("Abonelik iptal edilemedi.");
        }
        return NoContent();
    }

    [HttpPost("subscriptions/{id}/renew")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RenewSubscription(Guid id)
    {
        var success = await _subscriptionService.RenewSubscriptionAsync(id);
        if (!success)
        {
            return BadRequest("Abonelik yenilenemedi.");
        }
        return NoContent();
    }

    // Subscription Payments
    [HttpGet("subscriptions/{id}/payments")]
    public async Task<ActionResult<IEnumerable<SubscriptionPaymentDto>>> GetSubscriptionPayments(Guid id)
    {
        // ✅ ARCHITECTURE: Authorization check - kullanıcı sadece kendi subscription'ının payment'larını görebilir
        var userId = GetUserId();
        var subscription = await _subscriptionService.GetUserSubscriptionByIdAsync(id);
        if (subscription == null)
        {
            return NotFound();
        }

        if (subscription.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var payments = await _subscriptionService.GetSubscriptionPaymentsAsync(id);
        return Ok(payments);
    }

    [HttpPost("payments/{id}/process")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ProcessPayment(Guid id, [FromBody] ProcessSubscriptionPaymentDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var success = await _subscriptionService.ProcessPaymentAsync(id, dto.TransactionId);
        if (!success)
        {
            return BadRequest("Ödeme işlenemedi.");
        }
        return NoContent();
    }

    [HttpPost("payments/{id}/retry")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RetryPayment(Guid id)
    {
        var success = await _subscriptionService.RetryFailedPaymentAsync(id);
        if (!success)
        {
            return BadRequest("Ödeme tekrar denenemedi.");
        }
        return NoContent();
    }

    // Subscription Usage
    [HttpPost("usage/track")]
    public async Task<ActionResult<SubscriptionUsageDto>> TrackUsage([FromBody] TrackUsageDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var subscription = await _subscriptionService.GetUserActiveSubscriptionAsync(userId);
        if (subscription == null)
        {
            return BadRequest("Aktif abonelik bulunamadı.");
        }

        var usage = await _subscriptionService.TrackUsageAsync(subscription.Id, dto.Feature, dto.Count);
        return Ok(usage);
    }

    [HttpGet("usage")]
    public async Task<ActionResult<IEnumerable<SubscriptionUsageDto>>> GetMyUsage()
    {
        var userId = GetUserId();
        var subscription = await _subscriptionService.GetUserActiveSubscriptionAsync(userId);
        if (subscription == null)
        {
            return BadRequest();
        }

        var usage = await _subscriptionService.GetAllUsageAsync(subscription.Id);
        return Ok(usage);
    }

    // Analytics
    [HttpGet("analytics")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<SubscriptionAnalyticsDto>> GetAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var analytics = await _subscriptionService.GetSubscriptionAnalyticsAsync(startDate, endDate);
        return Ok(analytics);
    }

    [HttpGet("trends")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<SubscriptionTrendDto>>> GetTrends([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var trends = await _subscriptionService.GetSubscriptionTrendsAsync(startDate, endDate);
        return Ok(trends);
    }
}

