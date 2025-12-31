using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Support;
using Merge.Application.DTOs.Support;


namespace Merge.API.Controllers.Support;

[ApiController]
[Route("api/support/communications")]
[Authorize]
public class CustomerCommunicationsController : BaseController
{
    private readonly ICustomerCommunicationService _customerCommunicationService;

    public CustomerCommunicationsController(ICustomerCommunicationService customerCommunicationService)
    {
        _customerCommunicationService = customerCommunicationService;
    }

    [HttpGet("my-communications")]
    public async Task<ActionResult<IEnumerable<CustomerCommunicationDto>>> GetMyCommunications(
        [FromQuery] string? communicationType = null,
        [FromQuery] string? channel = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var communications = await _customerCommunicationService.GetUserCommunicationsAsync(userId, communicationType, channel, page, pageSize);
        return Ok(communications);
    }

    [HttpGet("my-history")]
    public async Task<ActionResult<CommunicationHistoryDto>> GetMyHistory()
    {
        var userId = GetUserId();
        var history = await _customerCommunicationService.GetUserCommunicationHistoryAsync(userId);
        return Ok(history);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerCommunicationDto>> GetCommunication(Guid id)
    {
        var communication = await _customerCommunicationService.GetCommunicationAsync(id);
        if (communication == null)
        {
            return NotFound();
        }
        return Ok(communication);
    }

    // Admin endpoints
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Support")]
    public async Task<ActionResult<CustomerCommunicationDto>> CreateCommunication([FromBody] CreateCustomerCommunicationDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var sentByUserId = GetUserIdOrNull();
        var communication = await _customerCommunicationService.CreateCommunicationAsync(dto, sentByUserId);
        return CreatedAtAction(nameof(GetCommunication), new { id = communication.Id }, communication);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Support")]
    public async Task<ActionResult<IEnumerable<CustomerCommunicationDto>>> GetAllCommunications(
        [FromQuery] string? communicationType = null,
        [FromQuery] string? channel = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var communications = await _customerCommunicationService.GetAllCommunicationsAsync(
            communicationType, channel, userId, startDate, endDate, page, pageSize);
        return Ok(communications);
    }

    [HttpGet("user/{userId}/history")]
    [Authorize(Roles = "Admin,Manager,Support")]
    public async Task<ActionResult<CommunicationHistoryDto>> GetUserHistory(Guid userId)
    {
        var history = await _customerCommunicationService.GetUserCommunicationHistoryAsync(userId);
        return Ok(history);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Manager,Support")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateCommunicationStatusDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var success = await _customerCommunicationService.UpdateCommunicationStatusAsync(
            id, dto.Status, dto.DeliveredAt, dto.ReadAt);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<Dictionary<string, int>>> GetStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var stats = await _customerCommunicationService.GetCommunicationStatsAsync(startDate, endDate);
        return Ok(stats);
    }
}

