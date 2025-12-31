using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;


namespace Merge.API.Controllers.Notification;

[ApiController]
[Route("api/notifications/templates")]
[Authorize(Roles = "Admin,Manager")]
public class NotificationTemplatesController : BaseController
{
    private readonly INotificationTemplateService _templateService;

    public NotificationTemplatesController(INotificationTemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpPost]
    public async Task<ActionResult<NotificationTemplateDto>> CreateTemplate([FromBody] CreateNotificationTemplateDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var template = await _templateService.CreateTemplateAsync(dto);
        return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, template);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NotificationTemplateDto>> GetTemplate(Guid id)
    {
        var template = await _templateService.GetTemplateAsync(id);
        if (template == null)
        {
            return NotFound();
        }
        return Ok(template);
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<NotificationTemplateDto>> GetTemplateByType(string type)
    {
        var template = await _templateService.GetTemplateByTypeAsync(type);
        if (template == null)
        {
            return NotFound(new { message = $"Template not found for type: {type}" });
        }
        return Ok(template);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationTemplateDto>>> GetTemplates([FromQuery] string? type = null)
    {
        var templates = await _templateService.GetTemplatesAsync(type);
        return Ok(templates);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<NotificationTemplateDto>> UpdateTemplate(Guid id, [FromBody] UpdateNotificationTemplateDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var template = await _templateService.UpdateTemplateAsync(id, dto);
        if (template == null)
        {
            return NotFound();
        }
        return Ok(template);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTemplate(Guid id)
    {
        var success = await _templateService.DeleteTemplateAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("create-notification")]
    [Authorize]
    public async Task<ActionResult<NotificationDto>> CreateNotificationFromTemplate(
        [FromBody] CreateNotificationFromTemplateDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var notification = await _templateService.CreateNotificationFromTemplateAsync(dto.UserId, dto.TemplateType, dto.Variables);
        return Ok(notification);
    }
}

