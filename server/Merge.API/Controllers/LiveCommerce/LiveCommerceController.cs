using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.LiveCommerce;
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
using Merge.Application.LiveCommerce.Commands.DeleteLiveStream;
using Merge.Application.LiveCommerce.Commands.PauseStream;
using Merge.Application.LiveCommerce.Commands.ResumeStream;
using Merge.Application.LiveCommerce.Commands.CancelStream;

namespace Merge.API.Controllers.LiveCommerce;

// ✅ BOLUM 4.1: API Versioning (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/live-commerce")]
public class LiveCommerceController(IMediator mediator) : BaseController
{

    /// <summary>
    /// Yeni canlı yayın oluşturur (Seller, Admin)
    /// </summary>
    [HttpPost("streams")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(LiveStreamDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LiveStreamDto>> CreateStream(
        [FromBody] CreateLiveStreamDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.2: IDOR Koruması - Seller sadece kendi stream'ini oluşturabilir
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // Seller kontrolü - Seller sadece kendi ID'sini kullanabilir
        if (!TryGetUserRole(out var role) || (role != "Admin" && dto.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inizi oluşturabilirsiniz.");
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.3: ValidationBehavior otomatik olarak ValidateModelState'i handle ediyor
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

    /// <summary>
    /// Canlı yayın detaylarını günceller (Seller, Admin)
    /// </summary>
    [HttpPut("streams/{id}")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
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
        // ✅ BOLUM 3.2: IDOR Koruması - Seller sadece kendi stream'ini güncelleyebilir
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var streamQuery = new GetLiveStreamQuery(id);
        var existingStream = await mediator.Send(streamQuery, cancellationToken);
        if (existingStream == null)
        {
            return NotFound();
        }

        if (!TryGetUserRole(out var role) || (role != "Admin" && existingStream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inizi güncelleyebilirsiniz.");
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.3: ValidationBehavior otomatik olarak ValidateModelState'i handle ediyor
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
    /// Canlı yayını siler (Seller, Admin)
    /// </summary>
    [HttpDelete("streams/{id}")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika (kritik işlem)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteStream(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.2: IDOR Koruması - Seller sadece kendi stream'ini silebilir
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var streamQuery = new GetLiveStreamQuery(id);
        var stream = await mediator.Send(streamQuery, cancellationToken);
        if (stream == null)
        {
            return NotFound();
        }

        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inizi silebilirsiniz.");
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var command = new DeleteLiveStreamCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Canlı yayın detaylarını getirir
    /// </summary>
    [HttpGet("streams/{id}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(LiveStreamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LiveStreamDto>> GetStream(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetLiveStreamQuery(id);
        var stream = await mediator.Send(query, cancellationToken);
        if (stream == null)
        {
            return NotFound();
        }
        return Ok(stream);
    }

    /// <summary>
    /// Aktif canlı yayınları getirir
    /// </summary>
    [HttpGet("streams/active")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<LiveStreamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<LiveStreamDto>>> GetActiveStreams(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetActiveStreamsQuery(page, pageSize);
        var streams = await mediator.Send(query, cancellationToken);
        return Ok(streams);
    }

    /// <summary>
    /// Satıcıya ait canlı yayınları getirir
    /// </summary>
    [HttpGet("streams/seller/{sellerId}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<LiveStreamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<LiveStreamDto>>> GetStreamsBySeller(
        Guid sellerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetStreamsBySellerQuery(sellerId, page, pageSize);
        var streams = await mediator.Send(query, cancellationToken);
        return Ok(streams);
    }

    /// <summary>
    /// Canlı yayını başlatır (Seller, Admin)
    /// </summary>
    [HttpPost("streams/{id}/start")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika (kritik işlem)
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

        // ✅ BOLUM 3.2: IDOR Koruması - Seller sadece kendi stream'ini başlatabilir
        var streamQuery = new GetLiveStreamQuery(id);
        var stream = await mediator.Send(streamQuery, cancellationToken);
        if (stream == null)
        {
            return NotFound();
        }

        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inizi başlatabilirsiniz.");
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var command = new StartStreamCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Canlı yayını sonlandırır (Seller, Admin)
    /// </summary>
    [HttpPost("streams/{id}/end")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika (kritik işlem)
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

        // ✅ BOLUM 3.2: IDOR Koruması - Seller sadece kendi stream'ini sonlandırabilir
        var streamQuery = new GetLiveStreamQuery(id);
        var stream = await mediator.Send(streamQuery, cancellationToken);
        if (stream == null)
        {
            return NotFound();
        }

        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inizi sonlandırabilirsiniz.");
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var command = new EndStreamCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Canlı yayını duraklatır (Seller, Admin)
    /// </summary>
    [HttpPost("streams/{id}/pause")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika (kritik işlem)
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

        // ✅ BOLUM 3.2: IDOR Koruması - Seller sadece kendi stream'ini duraklatabilir
        var streamQuery = new GetLiveStreamQuery(id);
        var stream = await mediator.Send(streamQuery, cancellationToken);
        if (stream == null)
        {
            return NotFound();
        }

        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inizi duraklatabilirsiniz.");
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var command = new PauseStreamCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Duraklatılmış canlı yayını devam ettirir (Seller, Admin)
    /// </summary>
    [HttpPost("streams/{id}/resume")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika (kritik işlem)
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

        // ✅ BOLUM 3.2: IDOR Koruması - Seller sadece kendi stream'ini devam ettirebilir
        var streamQuery = new GetLiveStreamQuery(id);
        var stream = await mediator.Send(streamQuery, cancellationToken);
        if (stream == null)
        {
            return NotFound();
        }

        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inizi devam ettirebilirsiniz.");
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var command = new ResumeStreamCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Canlı yayını iptal eder (Seller, Admin)
    /// </summary>
    [HttpPost("streams/{id}/cancel")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika (kritik işlem)
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

        // ✅ BOLUM 3.2: IDOR Koruması - Seller sadece kendi stream'ini iptal edebilir
        var streamQuery = new GetLiveStreamQuery(id);
        var stream = await mediator.Send(streamQuery, cancellationToken);
        if (stream == null)
        {
            return NotFound();
        }

        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inizi iptal edebilirsiniz.");
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var command = new CancelStreamCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Canlı yayına ürün ekler (Seller, Admin)
    /// </summary>
    [HttpPost("streams/{streamId}/products/{productId}")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
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

        // ✅ BOLUM 3.2: IDOR Koruması - Seller sadece kendi stream'ine ürün ekleyebilir
        var streamQuery = new GetLiveStreamQuery(streamId);
        var stream = await mediator.Send(streamQuery, cancellationToken);
        if (stream == null)
        {
            return NotFound();
        }

        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inize ürün ekleyebilirsiniz.");
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.3: ValidationBehavior otomatik olarak ValidateModelState'i handle ediyor
        var command = new AddProductToStreamCommand(
            streamId,
            productId,
            dto?.DisplayOrder ?? 0,
            dto?.SpecialPrice,
            dto?.ShowcaseNotes);
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Ürünü vitrine çıkarır (Seller, Admin)
    /// </summary>
    [HttpPost("streams/{streamId}/products/{productId}/showcase")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
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

        // ✅ BOLUM 3.2: IDOR Koruması - Seller sadece kendi stream'inde ürün vitrine çıkarabilir
        var streamQuery = new GetLiveStreamQuery(streamId);
        var stream = await mediator.Send(streamQuery, cancellationToken);
        if (stream == null)
        {
            return NotFound();
        }

        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inizde ürün vitrine çıkarabilirsiniz.");
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var command = new ShowcaseProductCommand(streamId, productId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Canlı yayına katılır
    /// </summary>
    [HttpPost("streams/{streamId}/join")]
    [AllowAnonymous]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> JoinStream(
        Guid streamId,
        [FromBody] JoinStreamDto? dto = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserIdOrNull();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.3: ValidationBehavior otomatik olarak ValidateModelState'i handle ediyor
        var command = new JoinStreamCommand(streamId, userId, dto?.GuestId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Canlı yayından ayrılır
    /// </summary>
    [HttpPost("streams/{streamId}/leave")]
    [AllowAnonymous]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> LeaveStream(
        Guid streamId,
        [FromBody] LeaveStreamDto? dto = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserIdOrNull();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.3: ValidationBehavior otomatik olarak ValidateModelState'i handle ediyor
        var command = new LeaveStreamCommand(streamId, userId, dto?.GuestId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Canlı yayından sipariş oluşturur
    /// </summary>
    [HttpPost("streams/{streamId}/orders/{orderId}")]
    [Authorize]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var command = new CreateOrderFromStreamCommand(streamId, orderId, productId);
        var streamOrder = await mediator.Send(command, cancellationToken);
        return Ok(streamOrder);
    }

    /// <summary>
    /// Canlı yayın istatistiklerini getirir
    /// </summary>
    [HttpGet("streams/{id}/stats")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
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

        // ✅ BOLUM 3.2: IDOR Koruması - Seller sadece kendi stream istatistiklerini görebilir
        var streamQuery = new GetLiveStreamQuery(id);
        var stream = await mediator.Send(streamQuery, cancellationToken);
        if (stream == null)
        {
            return NotFound();
        }

        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream istatistiklerinizi görebilirsiniz.");
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetStreamStatsQuery(id);
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }
}

