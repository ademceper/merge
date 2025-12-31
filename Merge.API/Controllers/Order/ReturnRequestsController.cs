using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Order;
using Merge.Application.DTOs.Order;


namespace Merge.API.Controllers.Order;

[ApiController]
[Route("api/orders/return-requests")]
[Authorize]
public class ReturnRequestsController : BaseController
{
    private readonly IReturnRequestService _returnRequestService;

    public ReturnRequestsController(IReturnRequestService returnRequestService)
    {
        _returnRequestService = returnRequestService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReturnRequestDto>>> GetMyReturns()
    {
        var userId = GetUserId();
        var returns = await _returnRequestService.GetByUserIdAsync(userId);
        return Ok(returns);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<ReturnRequestDto>>> GetAll([FromQuery] string? status = null)
    {
        var returns = await _returnRequestService.GetAllAsync(status);
        return Ok(returns);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReturnRequestDto>> GetById(Guid id)
    {
        var userId = GetUserId();
        var returnRequest = await _returnRequestService.GetByIdAsync(id);
        if (returnRequest == null)
        {
            return NotFound();
        }
        
        // ✅ SECURITY: Authorization check - Kullanıcı sadece kendi iade taleplerine erişebilmeli
        if (returnRequest.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        
        return Ok(returnRequest);
    }

    [HttpPost]
    public async Task<ActionResult<ReturnRequestDto>> Create([FromBody] CreateReturnRequestDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        dto.UserId = userId;
        var returnRequest = await _returnRequestService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = returnRequest.Id }, returnRequest);
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _returnRequestService.ApproveAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectReturnDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var result = await _returnRequestService.RejectAsync(id, dto.Reason);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/complete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteReturnDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var result = await _returnRequestService.CompleteAsync(id, dto.TrackingNumber);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

