using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.User;
using Merge.Application.User.Commands.ResetUserPreference;
using Merge.Application.User.Commands.UpdateUserPreference;
using Merge.Application.User.Queries.GetUserPreference;
using Merge.API.Middleware;

namespace Merge.API.Controllers.User;

/// <summary>
/// User Preferences API endpoints.
/// Kullanıcı tercihlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/user/preferences")]
[Authorize]
[Tags("UserPreferences")]
public class PreferencesController(IMediator mediator) : BaseController
{

    [HttpGet]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(UserPreferenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<UserPreferenceDto>> GetMyPreferences(CancellationToken cancellationToken = default)
    {
                if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

                var query = new GetUserPreferenceQuery(userId);
        var preferences = await mediator.Send(query, cancellationToken);
        return Ok(preferences);
    }

    [HttpPut]
    [RateLimit(30, 60)]     [ProducesResponseType(typeof(UserPreferenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<UserPreferenceDto>> UpdateMyPreferences(
        [FromBody] UpdateUserPreferenceDto dto,
        CancellationToken cancellationToken = default)
    {
                var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

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
        var preferences = await mediator.Send(command, cancellationToken);
        return Ok(preferences);
    }

    /// <summary>
    /// Kullanıcı tercihlerini kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(UserPreferenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<UserPreferenceDto>> PatchMyPreferences(
        [FromBody] PatchUserPreferenceDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var command = new UpdateUserPreferenceCommand(
            userId,
            patchDto.Theme,
            patchDto.DefaultLanguage,
            patchDto.DefaultCurrency,
            patchDto.ItemsPerPage,
            patchDto.DateFormat,
            patchDto.TimeFormat,
            patchDto.EmailNotifications,
            patchDto.SmsNotifications,
            patchDto.PushNotifications,
            patchDto.OrderUpdates,
            patchDto.PromotionalEmails,
            patchDto.ProductRecommendations,
            patchDto.ReviewReminders,
            patchDto.WishlistPriceAlerts,
            patchDto.NewsletterSubscription,
            patchDto.ShowProfilePublicly,
            patchDto.ShowPurchaseHistory,
            patchDto.AllowPersonalization,
            patchDto.AllowDataCollection,
            patchDto.AllowThirdPartySharing,
            patchDto.DefaultShippingAddress,
            patchDto.DefaultPaymentMethod,
            patchDto.AutoApplyCoupons,
            patchDto.SaveCartOnLogout,
            patchDto.ShowOutOfStockItems);
        var preferences = await mediator.Send(command, cancellationToken);
        return Ok(preferences);
    }

    [HttpPost("reset")]
    [RateLimit(10, 3600)]     [ProducesResponseType(typeof(UserPreferenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<UserPreferenceDto>> ResetToDefaults(CancellationToken cancellationToken = default)
    {
                if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

                var command = new ResetUserPreferenceCommand(userId);
        var preferences = await mediator.Send(command, cancellationToken);
        return Ok(preferences);
    }
}
