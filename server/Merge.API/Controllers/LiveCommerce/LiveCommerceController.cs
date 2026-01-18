using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.LiveCommerce;
using Merge.Application.Exceptions;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.LiveCommerce.Commands.CreateLiveStream;
using Merge.Application.LiveCommerce.Queries.GetLiveStream;
using Merge.Application.LiveCommerce.Queries.GetActiveStreams;
using Merge.Application.LiveCommerce.Queries.GetStreamsBySeller;
using Merge.Application.LiveCommerce.Commands.StartStream;
using Merge.Application.LiveCommerce.Commands.EndStream;
using Merge.Application.LiveCommerce.Commands.AddProductToStream;
using Merge.Application.LiveCommerce.Commands.ShowcaseProduct;
using Merge.Application.LiveCommerce.Commands.JoinStream;
using Merge.Application.LiveCommerce.Commands.LeaveStream;
using Merge.Application.LiveCommerce.Commands.CreateOrderFromStream;
using Merge.Application.LiveCommerce.Queries.GetStreamStats;
using Merge.Application.LiveCommerce.Commands.UpdateLiveStream;
using Merge.Application.LiveCommerce.Commands.PatchLiveStream;
using Merge.Application.LiveCommerce.Commands.DeleteLiveStream;
using Merge.Application.LiveCommerce.Commands.PauseStream;
using Merge.Application.LiveCommerce.Commands.ResumeStream;
using Merge.Application.LiveCommerce.Commands.CancelStream;

namespace Merge.API.Controllers.LiveCommerce;

/// <summary>
/// Live Commerce API endpoints.
/// Canlı alışveriş yayınlarını yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/live-commerce")]
[Tags("LiveCommerce")]
public class LiveCommerceController(IMediator mediator) : BaseController
{
    [HttpPost("streams")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(LiveStreamDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LiveStreamDto>> CreateStream(
        [FromBody] CreateLiveStreamDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        if (!TryGetUserRole(out var role) || (role != "Admin" && dto.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inizi oluşturabilirsiniz.");
        }
        var command = new CreateLiveStreamCommand(
            dto.SellerId,
            dto.Title,
            dto.Description,
            dto.ScheduledStartTime,
            dto.StreamUrl,
            dto.StreamKey,
            dto.ThumbnailUrl,
            dto.Category,
            dto.Tags);
        var stream = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetStream), new { id = stream.Id }, stream);
    }

    [HttpPut("streams/{id}")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(LiveStreamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LiveStreamDto>> UpdateStream(
        Guid id,
        [FromBody] CreateLiveStreamDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var streamQuery = new GetLiveStreamQuery(id);
        var existingStream = await mediator.Send(streamQuery, cancellationToken)
            ?? throw new NotFoundException("LiveStream", id);
        if (!TryGetUserRole(out var role) || (role != "Admin" && existingStream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inizi güncelleyebilirsiniz.");
        }
        var command = new UpdateLiveStreamCommand(
            id,
            dto.Title,
            dto.Description,
            dto.ScheduledStartTime,
            dto.StreamUrl,
            dto.StreamKey,
            dto.ThumbnailUrl,
            dto.Category,
            dto.Tags);
        var stream = await mediator.Send(command, cancellationToken);
        return Ok(stream);
    }

    /// <summary>
    /// Canlı yayını kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("streams/{id}")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(LiveStreamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LiveStreamDto>> PatchStream(
        Guid id,
        [FromBody] PatchLiveStreamDto patchDto,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var streamQuery = new GetLiveStreamQuery(id);
        var existingStream = await mediator.Send(streamQuery, cancellationToken)
            ?? throw new NotFoundException("LiveStream", id);
        if (existingStream.SellerId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }
        var command = new PatchLiveStreamCommand(id, patchDto);
        var stream = await mediator.Send(command, cancellationToken);
        return Ok(stream);
    }

    [HttpDelete("streams/{id}")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(5, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteStream(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var streamQuery = new GetLiveStreamQuery(id);
        var stream = await mediator.Send(streamQuery, cancellationToken)
            ?? throw new NotFoundException("LiveStream", id);
        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inizi silebilirsiniz.");
        }
        var command = new DeleteLiveStreamCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("streams/{id}")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(LiveStreamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LiveStreamDto>> GetStream(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLiveStreamQuery(id);
        var stream = await mediator.Send(query, cancellationToken)
            ?? throw new NotFoundException("LiveStream", id);
        return Ok(stream);
    }

    [HttpGet("streams/active")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<LiveStreamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<LiveStreamDto>>> GetActiveStreams(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetActiveStreamsQuery(page, pageSize);
        var streams = await mediator.Send(query, cancellationToken);
        return Ok(streams);
    }

    [HttpGet("streams/seller/{sellerId}")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<LiveStreamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<LiveStreamDto>>> GetStreamsBySeller(
        Guid sellerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStreamsBySellerQuery(sellerId, page, pageSize);
        var streams = await mediator.Send(query, cancellationToken);
        return Ok(streams);
    }

    [HttpPost("streams/{id}/start")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(5, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> StartStream(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var streamQuery = new GetLiveStreamQuery(id);
        var stream = await mediator.Send(streamQuery, cancellationToken)
            ?? throw new NotFoundException("LiveStream", id);
        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inizi başlatabilirsiniz.");
        }
        var command = new StartStreamCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("streams/{id}/end")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(5, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> EndStream(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var streamQuery = new GetLiveStreamQuery(id);
        var stream = await mediator.Send(streamQuery, cancellationToken)
            ?? throw new NotFoundException("LiveStream", id);
        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inizi sonlandırabilirsiniz.");
        }
        var command = new EndStreamCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("streams/{id}/pause")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(5, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PauseStream(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var streamQuery = new GetLiveStreamQuery(id);
        var stream = await mediator.Send(streamQuery, cancellationToken)
            ?? throw new NotFoundException("LiveStream", id);
        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inizi duraklatabilirsiniz.");
        }
        var command = new PauseStreamCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("streams/{id}/resume")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(5, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ResumeStream(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var streamQuery = new GetLiveStreamQuery(id);
        var stream = await mediator.Send(streamQuery, cancellationToken)
            ?? throw new NotFoundException("LiveStream", id);
        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inizi devam ettirebilirsiniz.");
        }
        var command = new ResumeStreamCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("streams/{id}/cancel")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(5, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CancelStream(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var streamQuery = new GetLiveStreamQuery(id);
        var stream = await mediator.Send(streamQuery, cancellationToken)
            ?? throw new NotFoundException("LiveStream", id);
        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inizi iptal edebilirsiniz.");
        }
        var command = new CancelStreamCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("streams/{streamId}/products/{productId}")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(LiveStreamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LiveStreamDto>> AddProduct(
        Guid streamId,
        Guid productId,
        [FromBody] AddProductToStreamDto? dto = null,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var streamQuery = new GetLiveStreamQuery(streamId);
        var stream = await mediator.Send(streamQuery, cancellationToken)
            ?? throw new NotFoundException("LiveStream", streamId);
        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inize ürün ekleyebilirsiniz.");
        }
        var command = new AddProductToStreamCommand(
            streamId,
            productId,
            dto?.DisplayOrder ?? 0,
            dto?.SpecialPrice,
            dto?.ShowcaseNotes);
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("streams/{streamId}/products/{productId}/showcase")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(20, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ShowcaseProduct(
        Guid streamId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var streamQuery = new GetLiveStreamQuery(streamId);
        var stream = await mediator.Send(streamQuery, cancellationToken)
            ?? throw new NotFoundException("LiveStream", streamId);
        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inizde ürün vitrine çıkarabilirsiniz.");
        }
        var command = new ShowcaseProductCommand(streamId, productId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("streams/{streamId}/join")]
    [AllowAnonymous]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> JoinStream(
        Guid streamId,
        [FromBody] JoinStreamDto? dto = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserIdOrNull();
        var command = new JoinStreamCommand(streamId, userId, dto?.GuestId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("streams/{streamId}/leave")]
    [AllowAnonymous]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> LeaveStream(
        Guid streamId,
        [FromBody] LeaveStreamDto? dto = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserIdOrNull();
        var command = new LeaveStreamCommand(streamId, userId, dto?.GuestId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("streams/{streamId}/orders/{orderId}")]
    [Authorize]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(LiveStreamOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LiveStreamOrderDto>> CreateOrderFromStream(
        Guid streamId,
        Guid orderId,
        [FromQuery] Guid? productId = null,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateOrderFromStreamCommand(streamId, orderId, productId);
        var streamOrder = await mediator.Send(command, cancellationToken);
        return Ok(streamOrder);
    }

    [HttpGet("streams/{id}/stats")]
    [Authorize]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(LiveStreamStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LiveStreamStatsDto>> GetStreamStats(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var streamQuery = new GetLiveStreamQuery(id);
        var stream = await mediator.Send(streamQuery, cancellationToken)
            ?? throw new NotFoundException("LiveStream", id);
        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream istatistiklerinizi görebilirsiniz.");
        }

        var query = new GetStreamStatsQuery(id);
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }
}
