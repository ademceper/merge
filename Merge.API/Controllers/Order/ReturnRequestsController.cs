using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Order;
using Merge.Application.DTOs.Order;
using Merge.Application.Common;


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

    // ✅ PERFORMANCE: Pagination eklendi - unbounded query önleme
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ReturnRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<ReturnRequestDto>>> GetMyReturns(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100; // Max limit
        if (page < 1) page = 1;
        
        var userId = GetUserId();
        var returns = await _returnRequestService.GetByUserIdAsync(userId, page, pageSize, cancellationToken);
        return Ok(returns);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<ReturnRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ReturnRequestDto>>> GetAll([FromQuery] string? status = null)
    {
        var returns = await _returnRequestService.GetAllAsync(status);
        return Ok(returns);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReturnRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
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
    [ProducesResponseType(typeof(ReturnRequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
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

