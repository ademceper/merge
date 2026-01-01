using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Content;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;


namespace Merge.API.Controllers.Content;

[ApiController]
[Route("api/content/banners")]
public class BannersController : BaseController
{
    private readonly IBannerService _bannerService;
        public BannersController(IBannerService bannerService)
    {
        _bannerService = bannerService;
            }

    [HttpGet]
    public async Task<ActionResult<PagedResult<BannerDto>>> GetActive(
        [FromQuery] string? position = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var banners = await _bannerService.GetActiveBannersAsync(position, page, pageSize);
        return Ok(banners);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<BannerDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var banners = await _bannerService.GetAllAsync(page, pageSize);
        return Ok(banners);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BannerDto>> GetById(Guid id)
    {
        var banner = await _bannerService.GetByIdAsync(id);
        if (banner == null)
        {
            return NotFound();
        }
        return Ok(banner);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BannerDto>> Create([FromBody] CreateBannerDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var banner = await _bannerService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = banner.Id }, banner);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BannerDto>> Update(Guid id, [FromBody] UpdateBannerDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var banner = await _bannerService.UpdateAsync(id, dto);
        if (banner == null)
        {
            return NotFound();
        }
        return Ok(banner);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _bannerService.DeleteAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

