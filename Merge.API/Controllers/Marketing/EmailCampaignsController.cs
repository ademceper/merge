using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.DTOs.Marketing;


namespace Merge.API.Controllers.Marketing;

[ApiController]
[Route("api/marketing/email-campaigns")]
[Authorize]
public class EmailCampaignsController : BaseController
{
    private readonly IEmailCampaignService _campaignService;

    public EmailCampaignsController(IEmailCampaignService campaignService)
    {
        _campaignService = campaignService;
    }

    // Campaign Management
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<EmailCampaignDto>> CreateCampaign([FromBody] CreateEmailCampaignDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var campaign = await _campaignService.CreateCampaignAsync(dto);
        return CreatedAtAction(nameof(GetCampaign), new { id = campaign.Id }, campaign);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<EmailCampaignDto>> GetCampaign(Guid id)
    {
        var campaign = await _campaignService.GetCampaignAsync(id);

        if (campaign == null)
        {
            return NotFound();
        }

        return Ok(campaign);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<EmailCampaignDto>>> GetCampaigns(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var campaigns = await _campaignService.GetCampaignsAsync(status, page, pageSize);
        return Ok(campaigns);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<EmailCampaignDto>> UpdateCampaign(Guid id, [FromBody] UpdateEmailCampaignDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var campaign = await _campaignService.UpdateCampaignAsync(id, dto);
        if (campaign == null)
        {
            return NotFound();
        }
        return Ok(campaign);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteCampaign(Guid id)
    {
        var success = await _campaignService.DeleteCampaignAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id}/schedule")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ScheduleCampaign(Guid id, [FromBody] DateTime scheduledAt)
    {
        var success = await _campaignService.ScheduleCampaignAsync(id, scheduledAt);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id}/send")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> SendCampaign(Guid id)
    {
        var success = await _campaignService.SendCampaignAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id}/pause")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> PauseCampaign(Guid id)
    {
        var success = await _campaignService.PauseCampaignAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CancelCampaign(Guid id)
    {
        var success = await _campaignService.CancelCampaignAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("test-email")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> SendTestEmail([FromBody] SendTestEmailDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        await _campaignService.SendTestEmailAsync(dto);
        return NoContent();
    }

    // Template Management
    [HttpPost("templates")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<EmailTemplateDto>> CreateTemplate([FromBody] CreateEmailTemplateDto dto)
    {
        var template = await _campaignService.CreateTemplateAsync(dto);
        return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, template);
    }

    [HttpGet("templates/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<EmailTemplateDto>> GetTemplate(Guid id)
    {
        var template = await _campaignService.GetTemplateAsync(id);

        if (template == null)
        {
            return NotFound();
        }

        return Ok(template);
    }

    [HttpGet("templates")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<EmailTemplateDto>>> GetTemplates([FromQuery] string? type = null)
    {
        var templates = await _campaignService.GetTemplatesAsync(type);
        return Ok(templates);
    }

    [HttpPut("templates/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<EmailTemplateDto>> UpdateTemplate(Guid id, [FromBody] CreateEmailTemplateDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var template = await _campaignService.UpdateTemplateAsync(id, dto);
        if (template == null)
        {
            return NotFound();
        }
        return Ok(template);
    }

    [HttpDelete("templates/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteTemplate(Guid id)
    {
        var success = await _campaignService.DeleteTemplateAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    // Subscriber Management
    [HttpPost("subscribers")]
    [AllowAnonymous]
    public async Task<ActionResult<EmailSubscriberDto>> Subscribe([FromBody] CreateEmailSubscriberDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var subscriber = await _campaignService.SubscribeAsync(dto);
        return CreatedAtAction(nameof(GetSubscriber), new { id = subscriber.Id }, subscriber);
    }

    [HttpPost("subscribers/unsubscribe")]
    [AllowAnonymous]
    public async Task<IActionResult> Unsubscribe([FromQuery] string email)
    {
        var success = await _campaignService.UnsubscribeAsync(email);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("subscribers/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<EmailSubscriberDto>> GetSubscriber(Guid id)
    {
        var subscriber = await _campaignService.GetSubscriberAsync(id);

        if (subscriber == null)
        {
            return NotFound();
        }

        return Ok(subscriber);
    }

    [HttpGet("subscribers")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<EmailSubscriberDto>>> GetSubscribers(
        [FromQuery] bool? isSubscribed = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var subscribers = await _campaignService.GetSubscribersAsync(isSubscribed, page, pageSize);
        return Ok(subscribers);
    }

    [HttpPut("subscribers/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateSubscriber(Guid id, [FromBody] CreateEmailSubscriberDto dto)
    {
        var success = await _campaignService.UpdateSubscriberAsync(id, dto);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("subscribers/bulk-import")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> BulkImportSubscribers([FromBody] BulkImportSubscribersDto dto)
    {
        var count = await _campaignService.BulkImportSubscribersAsync(dto);
        return Ok(new { count });
    }

    // Analytics
    [HttpGet("{id}/analytics")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<EmailCampaignAnalyticsDto>> GetCampaignAnalytics(Guid id)
    {
        var analytics = await _campaignService.GetCampaignAnalyticsAsync(id);

        if (analytics == null)
        {
            return NotFound();
        }

        return Ok(analytics);
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<EmailCampaignStatsDto>> GetCampaignStats()
    {
        var stats = await _campaignService.GetCampaignStatsAsync();
        return Ok(stats);
    }

    [HttpPost("track/open")]
    [AllowAnonymous]
    public async Task<IActionResult> TrackEmailOpen([FromQuery] Guid campaignId, [FromQuery] Guid subscriberId)
    {
        await _campaignService.RecordEmailOpenAsync(campaignId, subscriberId);
        return NoContent();
    }

    [HttpPost("track/click")]
    [AllowAnonymous]
    public async Task<IActionResult> TrackEmailClick([FromQuery] Guid campaignId, [FromQuery] Guid subscriberId)
    {
        await _campaignService.RecordEmailClickAsync(campaignId, subscriberId);
        return NoContent();
    }

    // Automation
    [HttpPost("automations")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<EmailAutomationDto>> CreateAutomation([FromBody] CreateEmailAutomationDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var automation = await _campaignService.CreateAutomationAsync(dto);
        return CreatedAtAction(nameof(GetAutomations), new { id = automation.Id }, automation);
    }

    [HttpGet("automations")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<EmailAutomationDto>>> GetAutomations()
    {
        var automations = await _campaignService.GetAutomationsAsync();
        return Ok(automations);
    }

    [HttpPost("automations/{id}/toggle")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ToggleAutomation(Guid id, [FromQuery] bool isActive)
    {
        var success = await _campaignService.ToggleAutomationAsync(id, isActive);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("automations/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteAutomation(Guid id)
    {
        var success = await _campaignService.DeleteAutomationAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}
