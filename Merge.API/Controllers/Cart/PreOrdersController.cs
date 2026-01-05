using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Cart;
using Merge.Application.DTOs.Cart;
using Merge.Application.Common;
using Merge.API.Middleware;


namespace Merge.API.Controllers.Cart;

[ApiController]
[Route("api/cart/pre-orders")]
public class PreOrdersController : BaseController
{
    private readonly IPreOrderService _preOrderService;

    public PreOrdersController(IPreOrderService preOrderService)
    {
        _preOrderService = preOrderService;
    }

    /// <summary>
    /// Ön sipariş oluşturur
    /// </summary>
    [HttpPost]
    [Authorize]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika
    [ProducesResponseType(typeof(PreOrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PreOrderDto>> CreatePreOrder(
        [FromBody] CreatePreOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var preOrder = await _preOrderService.CreatePreOrderAsync(userId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetPreOrder), new { id = preOrder.Id }, preOrder);
    }

    /// <summary>
    /// Ön sipariş detaylarını getirir
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PreOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PreOrderDto>> GetPreOrder(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var preOrder = await _preOrderService.GetPreOrderAsync(id, cancellationToken);

        if (preOrder == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 3.2: IDOR Korumasi - Ownership check (ZORUNLU)
        if (preOrder.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        return Ok(preOrder);
    }

    /// <summary>
    /// Kullanıcının ön siparişlerini listeler
    /// </summary>
    [HttpGet("my-preorders")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<PreOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<PreOrderDto>>> GetMyPreOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var preOrders = await _preOrderService.GetUserPreOrdersAsync(userId, page, pageSize, cancellationToken);
        return Ok(preOrders);
    }

    /// <summary>
    /// Ön siparişi iptal eder
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CancelPreOrder(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR Korumasi - Ownership check (ZORUNLU)
        var preOrder = await _preOrderService.GetPreOrderAsync(id, cancellationToken);
        if (preOrder == null)
        {
            return NotFound();
        }

        if (preOrder.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var success = await _preOrderService.CancelPreOrderAsync(id, userId, cancellationToken);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Ön sipariş depozitosu öder
    /// </summary>
    [HttpPost("pay-deposit")]
    [Authorize]
    [RateLimit(3, 60)] // ✅ BOLUM 3.3: Rate Limiting - 3 istek / dakika (ödeme için düşük limit)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PayDeposit(
        [FromBody] PayPreOrderDepositDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR Korumasi - Ownership check (ZORUNLU)
        var preOrder = await _preOrderService.GetPreOrderAsync(dto.PreOrderId, cancellationToken);
        if (preOrder == null)
        {
            return NotFound();
        }

        if (preOrder.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var success = await _preOrderService.PayDepositAsync(userId, dto, cancellationToken);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Ön siparişi siparişe dönüştürür
    /// </summary>
    [HttpPost("{id}/convert")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ConvertToOrder(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var success = await _preOrderService.ConvertToOrderAsync(id, cancellationToken);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Ön sipariş hazır olduğunda bildirim gönderir
    /// </summary>
    [HttpPost("{id}/notify")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> NotifyAvailable(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _preOrderService.NotifyPreOrderAvailableAsync(id, cancellationToken);
        return NoContent();
    }

    // Campaigns
    /// <summary>
    /// Ön sipariş kampanyası oluşturur
    /// </summary>
    [HttpPost("campaigns")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika
    [ProducesResponseType(typeof(PreOrderCampaignDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PreOrderCampaignDto>> CreateCampaign(
        [FromBody] CreatePreOrderCampaignDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var campaign = await _preOrderService.CreateCampaignAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetCampaign), new { id = campaign.Id }, campaign);
    }

    /// <summary>
    /// Ön sipariş kampanyası detaylarını getirir
    /// </summary>
    [HttpGet("campaigns/{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PreOrderCampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PreOrderCampaignDto>> GetCampaign(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var campaign = await _preOrderService.GetCampaignAsync(id, cancellationToken);

        if (campaign == null)
        {
            return NotFound();
        }

        return Ok(campaign);
    }

    /// <summary>
    /// Aktif ön sipariş kampanyalarını listeler
    /// </summary>
    [HttpGet("campaigns")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<PreOrderCampaignDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<PreOrderCampaignDto>>> GetActiveCampaigns(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var campaigns = await _preOrderService.GetActiveCampaignsAsync(page, pageSize, cancellationToken);
        return Ok(campaigns);
    }

    /// <summary>
    /// Ürüne göre ön sipariş kampanyalarını listeler
    /// </summary>
    [HttpGet("campaigns/product/{productId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<PreOrderCampaignDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<PreOrderCampaignDto>>> GetCampaignsByProduct(
        Guid productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var campaigns = await _preOrderService.GetCampaignsByProductAsync(productId, page, pageSize, cancellationToken);
        return Ok(campaigns);
    }

    /// <summary>
    /// Ön sipariş kampanyasını günceller
    /// </summary>
    [HttpPut("campaigns/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateCampaign(
        Guid id,
        [FromBody] CreatePreOrderCampaignDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var success = await _preOrderService.UpdateCampaignAsync(id, dto, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Ön sipariş kampanyasını devre dışı bırakır
    /// </summary>
    [HttpDelete("campaigns/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeactivateCampaign(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var success = await _preOrderService.DeactivateCampaignAsync(id, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Ön sipariş istatistiklerini getirir
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(PreOrderStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PreOrderStatsDto>> GetStats(CancellationToken cancellationToken = default)
    {
        var stats = await _preOrderService.GetPreOrderStatsAsync(cancellationToken);
        return Ok(stats);
    }
}
