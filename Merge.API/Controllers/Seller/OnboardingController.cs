using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Seller;
using Merge.Domain.Entities;
using Merge.Application.DTOs.Seller;

namespace Merge.API.Controllers.Seller;

[ApiController]
[Route("api/seller/onboarding")]
public class OnboardingController : BaseController
{
    private readonly ISellerOnboardingService _sellerOnboardingService;

    public OnboardingController(ISellerOnboardingService sellerOnboardingService)
    {
        _sellerOnboardingService = sellerOnboardingService;
    }

    [HttpPost("apply")]
    [Authorize]
    public async Task<ActionResult<SellerApplicationDto>> SubmitApplication([FromBody] CreateSellerApplicationDto applicationDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var application = await _sellerOnboardingService.SubmitApplicationAsync(userId, applicationDto);
        return CreatedAtAction(nameof(GetMyApplication), new { id = application.Id }, application);
    }

    [HttpGet("my-application")]
    [Authorize]
    public async Task<ActionResult<SellerApplicationDto>> GetMyApplication()
    {
        var userId = GetUserId();
        var application = await _sellerOnboardingService.GetUserApplicationAsync(userId);

        if (application == null)
        {
            return NotFound();
        }

        return Ok(application);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SellerApplicationDto>> GetApplication(Guid id)
    {
        var application = await _sellerOnboardingService.GetApplicationByIdAsync(id);

        if (application == null)
        {
            return NotFound();
        }

        return Ok(application);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<SellerApplicationDto>>> GetApplications(
        [FromQuery] SellerApplicationStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var applications = await _sellerOnboardingService.GetAllApplicationsAsync(status, page, pageSize);
        return Ok(applications);
    }

    [HttpPost("{id}/review")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SellerApplicationDto>> ReviewApplication(
        Guid id,
        [FromBody] ReviewSellerApplicationDto reviewDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var reviewerId = GetUserId();
        var application = await _sellerOnboardingService.ReviewApplicationAsync(id, reviewDto, reviewerId);
        if (application == null)
        {
            return NotFound();
        }
        return Ok(application);
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveApplication(Guid id)
    {
        var reviewerId = GetUserId();
        var result = await _sellerOnboardingService.ApproveApplicationAsync(id, reviewerId);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RejectApplication(Guid id, [FromBody] RejectApplicationDto rejectDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var reviewerId = GetUserId();
        var result = await _sellerOnboardingService.RejectApplicationAsync(id, rejectDto.Reason, reviewerId);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SellerOnboardingStatsDto>> GetStats()
    {
        var stats = await _sellerOnboardingService.GetOnboardingStatsAsync();
        return Ok(stats);
    }
}
