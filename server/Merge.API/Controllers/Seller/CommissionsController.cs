using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Seller;
using Merge.API.Middleware;
using Merge.API.Helpers;
using Merge.Application.Common;
using Merge.Domain.Enums;
using Merge.Application.Seller.Queries.GetCommission;
using Merge.Application.Seller.Queries.GetSellerCommissions;
using Merge.Application.Seller.Queries.GetAllCommissions;
using Merge.Application.Seller.Commands.ApproveCommission;
using Merge.Application.Seller.Commands.CancelCommission;
using Merge.Application.Seller.Commands.CreateCommissionTier;
using Merge.Application.Seller.Queries.GetAllCommissionTiers;
using Merge.Application.Seller.Commands.UpdateCommissionTier;
using Merge.Application.Seller.Commands.PatchCommissionTier;
using Merge.Application.Seller.Commands.DeleteCommissionTier;
using Merge.Application.Seller.Queries.GetSellerCommissionSettings;
using Merge.Application.Seller.Commands.UpdateSellerCommissionSettings;
using Merge.Application.Seller.Commands.RequestPayout;
using Merge.Application.Seller.Queries.GetPayout;
using Merge.Application.Seller.Queries.GetSellerPayouts;
using Merge.Application.Seller.Queries.GetAllPayouts;
using Merge.Application.Seller.Commands.ProcessPayout;
using Merge.Application.Seller.Commands.CompletePayout;
using Merge.Application.Seller.Commands.FailPayout;
using Merge.Application.Seller.Queries.GetCommissionStats;
using Merge.Application.Seller.Queries.GetAvailablePayoutAmount;

namespace Merge.API.Controllers.Seller;

/// <summary>
/// Seller Commissions API endpoints.
/// Satıcı komisyon işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/seller/commissions")]
[Tags("SellerCommissions")]
public class CommissionsController(IMediator mediator) : BaseController
{

    [HttpGet("{id}")]
    [Authorize]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(SellerCommissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SellerCommissionDto>> GetCommission(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var query = new GetCommissionQuery(id);
        var commission = await mediator.Send(query, cancellationToken);

        if (commission is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        if (commission.SellerId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        return Ok(commission);
    }

    [HttpGet("seller/{sellerId}")]
    [Authorize(Roles = "Admin,Manager,Seller")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<SellerCommissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<SellerCommissionDto>>> GetSellerCommissions(
        Guid sellerId,
        [FromQuery] CommissionStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var isSeller = User.IsInRole("Seller");

        // Sellers can only view their own commissions
        if (isSeller && TryGetUserId(out var userId) && userId != sellerId)
        {
            return Forbid();
        }
        var query = new GetSellerCommissionsQuery(sellerId, status);
        var commissions = await mediator.Send(query, cancellationToken);
        return Ok(commissions);
    }

    [HttpGet("my-commissions")]
    [Authorize(Roles = "Seller")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<SellerCommissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<SellerCommissionDto>>> GetMyCommissions(
        [FromQuery] CommissionStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var query = new GetSellerCommissionsQuery(userId, status);
        var commissions = await mediator.Send(query, cancellationToken);
        return Ok(commissions);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<SellerCommissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<SellerCommissionDto>>> GetAllCommissions(
        [FromQuery] CommissionStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllCommissionsQuery(status, page, pageSize);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ApproveCommission(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new ApproveCommissionCommand(id);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok();
    }

    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CancelCommission(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new CancelCommissionCommand(id);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok();
    }

    [HttpPost("tiers")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(CommissionTierDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CommissionTierDto>> CreateTier(
        [FromBody] CreateCommissionTierDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateCommissionTierCommand(
            dto.Name,
            dto.MinSales,
            dto.MaxSales,
            dto.CommissionRate,
            dto.PlatformFeeRate,
            dto.Priority);
        var tier = await mediator.Send(command, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetAllTiers), new { version }, tier);
    }

    [HttpGet("tiers")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<CommissionTierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<CommissionTierDto>>> GetAllTiers(
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllCommissionTiersQuery();
        var tiers = await mediator.Send(query, cancellationToken);
        return Ok(tiers);
    }

    [HttpPut("tiers/{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateTier(
        Guid id,
        [FromBody] CreateCommissionTierDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateCommissionTierCommand(
            id,
            dto.Name,
            dto.MinSales,
            dto.MaxSales,
            dto.CommissionRate,
            dto.PlatformFeeRate,
            dto.Priority);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok();
    }

    /// <summary>
    /// Komisyon seviyesini kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("tiers/{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PatchTier(
        Guid id,
        [FromBody] PatchCommissionTierDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var command = new PatchCommissionTierCommand(id, patchDto);
        var success = await mediator.Send(command, cancellationToken);
        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok();
    }

    [HttpDelete("tiers/{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteTier(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteCommissionTierCommand(id);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok();
    }

    [HttpGet("settings/{sellerId}")]
    [Authorize(Roles = "Admin,Manager,Seller")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(SellerCommissionSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SellerCommissionSettingsDto>> GetSellerSettings(
        Guid sellerId,
        CancellationToken cancellationToken = default)
    {
        var isSeller = User.IsInRole("Seller");

        if (isSeller && TryGetUserId(out var userId) && userId != sellerId)
        {
            return Forbid();
        }
        var query = new GetSellerCommissionSettingsQuery(sellerId);
        var settings = await mediator.Send(query, cancellationToken);

        if (settings is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok(settings);
    }

    [HttpPut("settings/{sellerId}")]
    [Authorize(Roles = "Admin,Manager,Seller")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(SellerCommissionSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SellerCommissionSettingsDto>> UpdateSellerSettings(
        Guid sellerId,
        [FromBody] UpdateCommissionSettingsDto dto,
        CancellationToken cancellationToken = default)
    {
        var isSeller = User.IsInRole("Seller");

        if (isSeller && TryGetUserId(out var userId) && userId != sellerId)
        {
            return Forbid();
        }
        var command = new UpdateSellerCommissionSettingsCommand(
            sellerId,
            dto.CustomCommissionRate,
            dto.UseCustomRate,
            dto.MinimumPayoutAmount,
            dto.PaymentMethod,
            dto.PaymentDetails);
        var settings = await mediator.Send(command, cancellationToken);
        return Ok(settings);
    }

    /// <summary>
    /// Satıcı komisyon ayarlarını kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("settings/{sellerId}")]
    [Authorize(Roles = "Admin,Manager,Seller")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(SellerCommissionSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SellerCommissionSettingsDto>> PatchSellerSettings(
        Guid sellerId,
        [FromBody] PatchCommissionSettingsDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;

        var isSeller = User.IsInRole("Seller");

        if (isSeller && TryGetUserId(out var userId) && userId != sellerId)
        {
            return Forbid();
        }

        var command = new UpdateSellerCommissionSettingsCommand(
            sellerId,
            patchDto.CustomCommissionRate,
            patchDto.UseCustomRate,
            patchDto.MinimumPayoutAmount,
            patchDto.PaymentMethod,
            patchDto.PaymentDetails);
        var settings = await mediator.Send(command, cancellationToken);
        return Ok(settings);
    }

    [HttpPost("payouts")]
    [Authorize(Roles = "Seller")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(CommissionPayoutDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CommissionPayoutDto>> RequestPayout(
        [FromBody] RequestPayoutDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var command = new RequestPayoutCommand(userId, dto.CommissionIds);
        var payout = await mediator.Send(command, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetPayout), new { version, id = payout.Id }, payout);
    }

    [HttpGet("payouts/{id}")]
    [Authorize]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(CommissionPayoutDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CommissionPayoutDto>> GetPayout(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var query = new GetPayoutQuery(id);
        var payout = await mediator.Send(query, cancellationToken);

        if (payout is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        if (payout.SellerId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        return Ok(payout);
    }

    [HttpGet("payouts/seller/{sellerId}")]
    [Authorize(Roles = "Admin,Manager,Seller")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<CommissionPayoutDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<CommissionPayoutDto>>> GetSellerPayouts(
        Guid sellerId,
        CancellationToken cancellationToken = default)
    {
        var isSeller = User.IsInRole("Seller");

        if (isSeller && TryGetUserId(out var userId) && userId != sellerId)
        {
            return Forbid();
        }
        var query = new GetSellerPayoutsQuery(sellerId);
        var payouts = await mediator.Send(query, cancellationToken);
        return Ok(payouts);
    }

    [HttpGet("my-payouts")]
    [Authorize(Roles = "Seller")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<CommissionPayoutDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<CommissionPayoutDto>>> GetMyPayouts(
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var query = new GetSellerPayoutsQuery(userId);
        var payouts = await mediator.Send(query, cancellationToken);
        return Ok(payouts);
    }

    [HttpGet("payouts")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<CommissionPayoutDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<CommissionPayoutDto>>> GetAllPayouts(
        [FromQuery] PayoutStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllPayoutsQuery(status, page, pageSize);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("payouts/{id}/process")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ProcessPayout(
        Guid id,
        [FromBody] ProcessPayoutDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new ProcessPayoutCommand(id, dto.TransactionReference);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok();
    }

    [HttpPost("payouts/{id}/complete")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CompletePayout(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new CompletePayoutCommand(id);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok();
    }

    [HttpPost("payouts/{id}/fail")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> FailPayout(
        Guid id,
        [FromBody] FailPayoutDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new FailPayoutCommand(id, dto.Reason);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok();
    }

    [HttpGet("stats")]
    [Authorize]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(CommissionStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CommissionStatsDto>> GetCommissionStats(
        [FromQuery] Guid? sellerId = null,
        CancellationToken cancellationToken = default)
    {
        var isSeller = User.IsInRole("Seller");

        // Sellers can only view their own stats
        if (isSeller && TryGetUserId(out var userId))
        {
            sellerId = userId;
        }
        var query = new GetCommissionStatsQuery(sellerId);
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Kullanılabilir ödeme tutarını getirir
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kullanılabilir ödeme tutarı</returns>
    /// <response code="200">Tutar başarıyla getirildi</response>
    /// <response code="401">Kimlik doğrulama gerekli</response>
    /// <response code="403">Yetki yok</response>
    /// <response code="429">Rate limit aşıldı</response>
    [HttpGet("available-payout")]
    [Authorize(Roles = "Seller")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<decimal>> GetAvailablePayoutAmount(
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var query = new GetAvailablePayoutAmountQuery(userId);
        var amount = await mediator.Send(query, cancellationToken);
        return Ok(amount);
    }
}
