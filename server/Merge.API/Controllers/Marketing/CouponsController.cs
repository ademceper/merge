using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Marketing.Queries.GetAllCoupons;
using Merge.Application.Marketing.Queries.GetCouponByCode;
using Merge.Application.Marketing.Queries.GetCouponById;
using Merge.Application.Marketing.Commands.CreateCoupon;
using Merge.Application.Marketing.Commands.UpdateCoupon;
using Merge.Application.Marketing.Commands.DeleteCoupon;
using Merge.Application.Marketing.Commands.ValidateCoupon;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.API.Controllers.Marketing;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/marketing/coupons")]
public class CouponsController(
    IMediator mediator,
    IOptions<MarketingSettings> marketingSettings) : BaseController
{
    private readonly MarketingSettings _marketingSettings = marketingSettings.Value;

    [HttpGet]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<CouponDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<CouponDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _marketingSettings.MaxPageSize) pageSize = _marketingSettings.MaxPageSize;

        var query = new GetAllCouponsQuery(PageNumber: page, PageSize: pageSize);
        var coupons = await mediator.Send(query, cancellationToken);
        return Ok(coupons);
    }

    [HttpGet("code/{code}")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CouponDto>> GetByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCouponByCodeQuery(code);
        var coupon = await mediator.Send(query, cancellationToken);
        
        if (coupon == null)
        {
            return NotFound();
        }
        
        return Ok(coupon);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CouponDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCouponByIdQuery(id);
        var coupon = await mediator.Send(query, cancellationToken);
        
        if (coupon == null)
        {
            return NotFound();
        }
        
        return Ok(coupon);
    }

    [HttpPost("validate")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<decimal>> ValidateCoupon(
        [FromBody] ValidateCouponDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new ValidateCouponCommand(
            dto.Code,
            dto.OrderAmount,
            dto.UserId,
            dto.ProductIds);
        
        var discount = await mediator.Send(command, cancellationToken);
        return Ok(new { discount });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CouponDto>> Create(
        [FromBody] CreateCouponDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateCouponCommand(
            dto.Code,
            dto.Description,
            dto.DiscountAmount,
            dto.DiscountPercentage,
            dto.StartDate,
            dto.EndDate,
            dto.UsageLimit,
            dto.MinimumPurchaseAmount,
            dto.MaximumDiscountAmount,
            dto.IsForNewUsersOnly,
            dto.ApplicableCategoryIds,
            dto.ApplicableProductIds);
        
        var coupon = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetByCode), new { code = coupon.Code }, coupon);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CouponDto>> Update(
        Guid id,
        [FromBody] UpdateCouponDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateCouponCommand(
            id,
            dto.Code,
            dto.Description,
            dto.DiscountAmount,
            dto.DiscountPercentage,
            dto.StartDate,
            dto.EndDate,
            dto.UsageLimit,
            dto.MinimumPurchaseAmount,
            dto.MaximumDiscountAmount,
            dto.IsActive,
            dto.IsForNewUsersOnly,
            dto.ApplicableCategoryIds,
            dto.ApplicableProductIds);
        
        var coupon = await mediator.Send(command, cancellationToken);
        return Ok(coupon);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteCouponCommand(id);
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}
