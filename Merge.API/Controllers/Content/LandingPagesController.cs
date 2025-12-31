using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Content;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;


namespace Merge.API.Controllers.Content;

[ApiController]
[Route("api/content/landing-pages")]
public class LandingPagesController : BaseController
{
    private readonly ILandingPageService _landingPageService;

    public LandingPagesController(ILandingPageService landingPageService)
    {
        _landingPageService = landingPageService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<LandingPageDto>> CreateLandingPage([FromBody] CreateLandingPageDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var authorId = GetUserId();
        var landingPage = await _landingPageService.CreateLandingPageAsync(authorId, dto);
        return CreatedAtAction(nameof(GetLandingPageById), new { id = landingPage.Id }, landingPage);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<LandingPageDto>> GetLandingPageById(Guid id)
    {
        var landingPage = await _landingPageService.GetLandingPageByIdAsync(id);
        if (landingPage == null)
        {
            return NotFound();
        }
        return Ok(landingPage);
    }

    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<LandingPageDto>> GetLandingPageBySlug(string slug)
    {
        var landingPage = await _landingPageService.GetLandingPageBySlugAsync(slug);
        if (landingPage == null)
        {
            return NotFound();
        }
        return Ok(landingPage);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<LandingPageDto>>> GetAllLandingPages([FromQuery] string? status = null, [FromQuery] bool? isActive = null)
    {
        var landingPages = await _landingPageService.GetAllLandingPagesAsync(status, isActive);
        return Ok(landingPages);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateLandingPage(Guid id, [FromBody] CreateLandingPageDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var result = await _landingPageService.UpdateLandingPageAsync(id, dto);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteLandingPage(Guid id)
    {
        var result = await _landingPageService.DeleteLandingPageAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/publish")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> PublishLandingPage(Guid id)
    {
        var result = await _landingPageService.PublishLandingPageAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/track-conversion")]
    [AllowAnonymous]
    public async Task<IActionResult> TrackConversion(Guid id)
    {
        var result = await _landingPageService.TrackConversionAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/create-variant")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<LandingPageDto>> CreateVariant(Guid id, [FromBody] CreateLandingPageDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var variant = await _landingPageService.CreateVariantAsync(id, dto);
        return CreatedAtAction(nameof(GetLandingPageById), new { id = variant.Id }, variant);
    }

    [HttpGet("{id}/analytics")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<LandingPageAnalyticsDto>> GetAnalytics(Guid id, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var analytics = await _landingPageService.GetLandingPageAnalyticsAsync(id, startDate, endDate);
        return Ok(analytics);
    }
}

