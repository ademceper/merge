using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.User;
using Merge.Application.User.Commands.ResetUserPreference;
using Merge.Application.User.Commands.UpdateUserPreference;
using Merge.Application.User.Queries.GetUserPreference;
using Merge.API.Middleware;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.2: IDOR koruması (ZORUNLU) - Kullanıcı sadece kendi tercihlerine erişebilir
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
namespace Merge.API.Controllers.User;

[ApiController]
[Route("api/v{version:apiVersion}/user/preferences")]
[Authorize]
public class PreferencesController : BaseController
{
    private readonly IMediator _mediator;

    public PreferencesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU) - Kullanıcı sadece kendi tercihlerine erişebilir
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(UserPreferenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<UserPreferenceDto>> GetMyPreferences(CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetUserPreferenceQuery(userId);
        var preferences = await _mediator.Send(query, cancellationToken);
        return Ok(preferences);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU) - Kullanıcı sadece kendi tercihlerini güncelleyebilir
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPut]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(UserPreferenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<UserPreferenceDto>> UpdateMyPreferences(
        [FromBody] UpdateUserPreferenceDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new UpdateUserPreferenceCommand(
            userId,
            dto.Theme,
            dto.DefaultLanguage,
            dto.DefaultCurrency,
            dto.ItemsPerPage,
            dto.DateFormat,
            dto.TimeFormat,
            dto.EmailNotifications,
            dto.SmsNotifications,
            dto.PushNotifications,
            dto.OrderUpdates,
            dto.PromotionalEmails,
            dto.ProductRecommendations,
            dto.ReviewReminders,
            dto.WishlistPriceAlerts,
            dto.NewsletterSubscription,
            dto.ShowProfilePublicly,
            dto.ShowPurchaseHistory,
            dto.AllowPersonalization,
            dto.AllowDataCollection,
            dto.AllowThirdPartySharing,
            dto.DefaultShippingAddress,
            dto.DefaultPaymentMethod,
            dto.AutoApplyCoupons,
            dto.SaveCartOnLogout,
            dto.ShowOutOfStockItems);
        var preferences = await _mediator.Send(command, cancellationToken);
        return Ok(preferences);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU) - Kullanıcı sadece kendi tercihlerini sıfırlayabilir
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("reset")]
    [RateLimit(10, 3600)] // ✅ BOLUM 3.3: Rate Limiting - 10/saat (reset işlemi sınırlı)
    [ProducesResponseType(typeof(UserPreferenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<UserPreferenceDto>> ResetToDefaults(CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new ResetUserPreferenceCommand(userId);
        var preferences = await _mediator.Send(command, cancellationToken);
        return Ok(preferences);
    }
}

