using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Order;
using Merge.Application.DTOs.Order;
using Merge.Application.Common;
using Merge.API.Middleware;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
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

    /// <summary>
    /// Kullanıcının iade taleplerini getirir (pagination ile)
    /// </summary>
    // ✅ PERFORMANCE: Pagination eklendi - unbounded query önleme
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<ReturnRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ReturnRequestDto>>> GetMyReturns(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100; // Max limit
        if (page < 1) page = 1;
        
        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var returns = await _returnRequestService.GetByUserIdAsync(userId, page, pageSize, cancellationToken);
        return Ok(returns);
    }

    /// <summary>
    /// Tüm iade taleplerini getirir (pagination ile) (Admin)
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<ReturnRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ReturnRequestDto>>> GetAll(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var returns = await _returnRequestService.GetAllAsync(status, page, pageSize, cancellationToken);
        return Ok(returns);
    }

    /// <summary>
    /// İade talebi detaylarını getirir
    /// </summary>
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(ReturnRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ReturnRequestDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var returnRequest = await _returnRequestService.GetByIdAsync(id, cancellationToken);
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

    /// <summary>
    /// Yeni iade talebi oluşturur
    /// </summary>
    [HttpPost]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika (Spam koruması)
    [ProducesResponseType(typeof(ReturnRequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ReturnRequestDto>> Create(
        [FromBody] CreateReturnRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        dto.UserId = userId;
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var returnRequest = await _returnRequestService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = returnRequest.Id }, returnRequest);
    }

    /// <summary>
    /// İade talebini onaylar (Admin)
    /// </summary>
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _returnRequestService.ApproveAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// İade talebini reddeder (Admin)
    /// </summary>
    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectReturnDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _returnRequestService.RejectAsync(id, dto.Reason, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// İade talebini tamamlar (Admin)
    /// </summary>
    [HttpPost("{id}/complete")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Complete(
        Guid id,
        [FromBody] CompleteReturnDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _returnRequestService.CompleteAsync(id, dto.TrackingNumber, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

