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
    public async Task<ActionResult<PagedResult<CouponDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var coupons = await _couponService.GetAllAsync(page, pageSize);
        return Ok(coupons);
    }

    [HttpGet("code/{code}")]
    public async Task<ActionResult<CouponDto>> GetByCode(string code)
    {
        var coupon = await _couponService.GetByCodeAsync(code);
        if (coupon == null)
        {
            return NotFound();
        }
        return Ok(coupon);
    }

    [HttpPost("validate")]
    public async Task<ActionResult<decimal>> ValidateCoupon([FromBody] ValidateCouponDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var discount = await _couponService.CalculateDiscountAsync(
            dto.Code, 
            dto.OrderAmount, 
            dto.UserId, 
            dto.ProductIds);
        return Ok(new { discount });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CouponDto>> Create([FromBody] CouponDto couponDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var coupon = await _couponService.CreateAsync(couponDto);
        return CreatedAtAction(nameof(GetByCode), new { code = coupon.Code }, coupon);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CouponDto>> Update(Guid id, [FromBody] CouponDto couponDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var coupon = await _couponService.UpdateAsync(id, couponDto);
        if (coupon == null)
        {
            return NotFound();
        }
        return Ok(coupon);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _couponService.DeleteAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

