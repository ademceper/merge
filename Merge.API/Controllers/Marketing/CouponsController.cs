using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;


namespace Merge.API.Controllers.Marketing;

[ApiController]
[Route("api/marketing/coupons")]
public class CouponsController : BaseController
{
    private readonly ICouponService _couponService;

    public CouponsController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CouponDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CouponDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var coupons = await _couponService.GetAllAsync(page, pageSize, cancellationToken);
        return Ok(coupons);
    }

    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CouponDto>> GetByCode(string code, CancellationToken cancellationToken = default)
    {
        var coupon = await _couponService.GetByCodeAsync(code, cancellationToken);
        if (coupon == null)
        {
            return NotFound();
        }
        return Ok(coupon);
    }

    [HttpPost("validate")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<decimal>> ValidateCoupon([FromBody] ValidateCouponDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var discount = await _couponService.CalculateDiscountAsync(
            dto.Code, 
            dto.OrderAmount, 
            dto.UserId, 
            dto.ProductIds,
            cancellationToken);
        return Ok(new { discount });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CouponDto>> Create([FromBody] CouponDto couponDto, CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var coupon = await _couponService.CreateAsync(couponDto, cancellationToken);
        return CreatedAtAction(nameof(GetByCode), new { code = coupon.Code }, coupon);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CouponDto>> Update(Guid id, [FromBody] CouponDto couponDto, CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var coupon = await _couponService.UpdateAsync(id, couponDto, cancellationToken);
        if (coupon == null)
        {
            return NotFound();
        }
        return Ok(coupon);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _couponService.DeleteAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

