using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Services;
using Merge.Application.Interfaces.LiveCommerce;
using Merge.Application.DTOs.LiveCommerce;
using Merge.Application.Common;
using Merge.API.Middleware;

namespace Merge.API.Controllers.LiveCommerce;

[ApiController]
[Route("api/live-commerce")]
[Authorize]
public class LiveCommerceController : BaseController
{
    private readonly ILiveCommerceService _liveCommerceService;
        public LiveCommerceController(ILiveCommerceService liveCommerceService)
    {
        _liveCommerceService = liveCommerceService;
            }

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
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

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

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var stream = await _liveCommerceService.CreateStreamAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetStream), new { id = stream.Id }, stream);
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var stream = await _liveCommerceService.GetStreamAsync(id, cancellationToken);
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
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        if (pageSize > 100) pageSize = 100; // Max limit

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var streams = await _liveCommerceService.GetActiveStreamsAsync(page, pageSize, cancellationToken);
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
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        if (pageSize > 100) pageSize = 100; // Max limit

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var streams = await _liveCommerceService.GetStreamsBySellerAsync(sellerId, page, pageSize, cancellationToken);
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
        var stream = await _liveCommerceService.GetStreamAsync(id, cancellationToken);
        if (stream == null)
        {
            return NotFound();
        }

        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inizi başlatabilirsiniz.");
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _liveCommerceService.StartStreamAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
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
        var stream = await _liveCommerceService.GetStreamAsync(id, cancellationToken);
        if (stream == null)
        {
            return NotFound();
        }

        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inizi sonlandırabilirsiniz.");
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _liveCommerceService.EndStreamAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
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
        if (dto != null)
        {
            var validationResult = ValidateModelState();
            if (validationResult != null) return validationResult;
        }

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Seller sadece kendi stream'ine ürün ekleyebilir
        var stream = await _liveCommerceService.GetStreamAsync(streamId, cancellationToken);
        if (stream == null)
        {
            return NotFound();
        }

        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inize ürün ekleyebilirsiniz.");
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _liveCommerceService.AddProductToStreamAsync(streamId, productId, dto, cancellationToken);
        if (result == null)
        {
            return NotFound();
        }
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
        var stream = await _liveCommerceService.GetStreamAsync(streamId, cancellationToken);
        if (stream == null)
        {
            return NotFound();
        }

        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream'inizde ürün vitrine çıkarabilirsiniz.");
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _liveCommerceService.ShowcaseProductAsync(streamId, productId, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
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
        if (dto != null)
        {
            var validationResult = ValidateModelState();
            if (validationResult != null) return validationResult;
        }

        var userId = GetUserIdOrNull();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _liveCommerceService.JoinStreamAsync(streamId, userId, dto?.GuestId, cancellationToken);
        if (!result)
        {
            return BadRequest("Yayına katılamadı.");
        }
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
        if (dto != null)
        {
            var validationResult = ValidateModelState();
            if (validationResult != null) return validationResult;
        }

        var userId = GetUserIdOrNull();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _liveCommerceService.LeaveStreamAsync(streamId, userId, dto?.GuestId, cancellationToken);
        if (!result)
        {
            return BadRequest("Yayından ayrılamadı.");
        }
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var streamOrder = await _liveCommerceService.CreateOrderFromStreamAsync(streamId, orderId, productId, cancellationToken);
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
        var stream = await _liveCommerceService.GetStreamAsync(id, cancellationToken);
        if (stream == null)
        {
            return NotFound();
        }

        if (!TryGetUserRole(out var role) || (role != "Admin" && stream.SellerId != userId))
        {
            return Forbid("Sadece kendi stream istatistiklerinizi görebilirsiniz.");
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var stats = await _liveCommerceService.GetStreamStatsAsync(id, cancellationToken);
        return Ok(stats);
    }
}

