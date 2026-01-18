using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Application.Common;
using Merge.Application.Order.Commands.CreateReturnRequest;
using Merge.Application.Order.Commands.ApproveReturnRequest;
using Merge.Application.Order.Commands.RejectReturnRequest;
using Merge.Application.Order.Commands.CompleteReturnRequest;
using Merge.Application.Order.Queries.GetReturnRequestById;
using Merge.Application.Order.Queries.GetReturnRequestsByUserId;
using Merge.Application.Order.Queries.GetAllReturnRequests;
using Merge.API.Middleware;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.API.Controllers.Order;

/// <summary>
/// Return Request API endpoints.
/// İade taleplerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/orders/return-requests")]
[Authorize]
[Tags("ReturnRequests")]
public class ReturnRequestsController(
    IMediator mediator,
    IOptions<OrderSettings> orderSettings) : BaseController
{
    private readonly OrderSettings _orderSettings = orderSettings.Value;
    [HttpGet]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<ReturnRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ReturnRequestDto>>> GetMyReturns(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _orderSettings.MaxPageSize) pageSize = _orderSettings.MaxPageSize;
        if (page < 1) page = 1;
        var userId = GetUserId();
        var query = new GetReturnRequestsByUserIdQuery(userId, page, pageSize);
        var returns = await mediator.Send(query, cancellationToken);
        return Ok(returns);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    [RateLimit(60, 60)]
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
        if (pageSize > _orderSettings.MaxPageSize) pageSize = _orderSettings.MaxPageSize;
        if (page < 1) page = 1;
        var query = new GetAllReturnRequestsQuery(status, page, pageSize);
        var returns = await mediator.Send(query, cancellationToken);
        return Ok(returns);
    }

    [HttpGet("{id}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(ReturnRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ReturnRequestDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetReturnRequestByIdQuery(id);
        var returnRequest = await mediator.Send(query, cancellationToken);
        if (returnRequest is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        if (returnRequest.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        return Ok(returnRequest);
    }

    [HttpPost]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(ReturnRequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ReturnRequestDto>> Create(
        [FromBody] CreateReturnRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;
        var userId = GetUserId();
        dto.UserId = userId;
        var command = new CreateReturnRequestCommand(dto);
        var returnRequest = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = returnRequest.Id }, returnRequest);
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken = default)
    {
        var command = new ApproveReturnRequestCommand(id);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
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
        if (validationResult is not null) return validationResult;
        var command = new RejectReturnRequestCommand(id, dto.Reason);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    [HttpPost("{id}/complete")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
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
        if (validationResult is not null) return validationResult;
        var command = new CompleteReturnRequestCommand(id, dto.TrackingNumber);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }
}
