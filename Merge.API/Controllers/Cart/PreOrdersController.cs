using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Cart;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Cart.Commands.CreatePreOrder;
using Merge.Application.Cart.Queries.GetPreOrder;
using Merge.Application.Cart.Queries.GetUserPreOrders;
using Merge.Application.Cart.Commands.CancelPreOrder;
using Merge.Application.Cart.Commands.PayPreOrderDeposit;
using Merge.Application.Cart.Commands.ConvertPreOrderToOrder;
using Merge.Application.Cart.Commands.NotifyPreOrderAvailable;
using Merge.Application.Cart.Commands.CreatePreOrderCampaign;
using Merge.Application.Cart.Queries.GetPreOrderCampaign;
using Merge.Application.Cart.Queries.GetActivePreOrderCampaigns;
using Merge.Application.Cart.Queries.GetPreOrderCampaignsByProduct;
using Merge.Application.Cart.Commands.UpdatePreOrderCampaign;
using Merge.Application.Cart.Commands.DeactivatePreOrderCampaign;
using Merge.Application.Cart.Queries.GetPreOrderStats;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Cart;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/cart/pre-orders")]
public class PreOrdersController : BaseController
{
    private readonly IMediator _mediator;
    private readonly PaginationSettings _paginationSettings;

    public PreOrdersController(
        IMediator mediator,
        IOptions<PaginationSettings> paginationSettings)
    {
        _mediator = mediator;
        _paginationSettings = paginationSettings.Value;
    }

    /// <summary>
    /// Ön sipariş oluşturur
    /// </summary>
    [HttpPost]
    [Authorize]
    [RateLimit(5, 60)]
    [ProducesResponseType(typeof(PreOrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PreOrderDto>> CreatePreOrder(
        [FromBody] CreatePreOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new CreatePreOrderCommand(
            userId,
            dto.ProductId,
            dto.Quantity,
            dto.VariantOptions,
            dto.Notes);
        var preOrder = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetPreOrder), new { id = preOrder.Id }, preOrder);
    }

    /// <summary>
    /// Ön sipariş detaylarını getirir
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PreOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PreOrderDto>> GetPreOrder(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetPreOrderQuery(id);
        var preOrder = await _mediator.Send(query, cancellationToken);

        if (preOrder == null)
        {
            return NotFound();
        }

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
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<PreOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<PreOrderDto>>> GetMyPreOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, _paginationSettings.MaxPageSize);
        if (page < 1) page = 1;

        var userId = GetUserId();
        var query = new GetUserPreOrdersQuery(userId, page, pageSize);
        var preOrders = await _mediator.Send(query, cancellationToken);
        return Ok(preOrders);
    }

    /// <summary>
    /// Ön siparişi iptal eder
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [RateLimit(5, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CancelPreOrder(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var preOrderQuery = new GetPreOrderQuery(id);
        var preOrder = await _mediator.Send(preOrderQuery, cancellationToken);
        if (preOrder == null)
        {
            return NotFound();
        }

        if (preOrder.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var command = new CancelPreOrderCommand(id, userId);
        var success = await _mediator.Send(command, cancellationToken);
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
    [RateLimit(3, 60)]
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
        var userId = GetUserId();
        var preOrderQuery = new GetPreOrderQuery(dto.PreOrderId);
        var preOrder = await _mediator.Send(preOrderQuery, cancellationToken);
        if (preOrder == null)
        {
            return NotFound();
        }

        if (preOrder.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var command = new PayPreOrderDepositCommand(userId, dto.PreOrderId, dto.Amount);
        var success = await _mediator.Send(command, cancellationToken);
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
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ConvertToOrder(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new ConvertPreOrderToOrderCommand(id);
        var success = await _mediator.Send(command, cancellationToken);
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
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> NotifyAvailable(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new NotifyPreOrderAvailableCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    // Campaigns
    /// <summary>
    /// Ön sipariş kampanyası oluşturur
    /// </summary>
    [HttpPost("campaigns")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(5, 60)]
    [ProducesResponseType(typeof(PreOrderCampaignDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PreOrderCampaignDto>> CreateCampaign(
        [FromBody] CreatePreOrderCampaignDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreatePreOrderCampaignCommand(
            dto.Name,
            dto.Description,
            dto.ProductId,
            dto.StartDate,
            dto.EndDate,
            dto.ExpectedDeliveryDate,
            dto.MaxQuantity,
            dto.DepositPercentage,
            dto.SpecialPrice,
            dto.NotifyOnAvailable);
        var campaign = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCampaign), new { id = campaign.Id }, campaign);
    }

    /// <summary>
    /// Ön sipariş kampanyası detaylarını getirir
    /// </summary>
    [HttpGet("campaigns/{id}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PreOrderCampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PreOrderCampaignDto>> GetCampaign(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPreOrderCampaignQuery(id);
        var campaign = await _mediator.Send(query, cancellationToken);

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
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<PreOrderCampaignDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<PreOrderCampaignDto>>> GetActiveCampaigns(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, _paginationSettings.MaxPageSize);
        if (page < 1) page = 1;

        var query = new GetActivePreOrderCampaignsQuery(page, pageSize);
        var campaigns = await _mediator.Send(query, cancellationToken);
        return Ok(campaigns);
    }

    /// <summary>
    /// Ürüne göre ön sipariş kampanyalarını listeler
    /// </summary>
    [HttpGet("campaigns/product/{productId}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<PreOrderCampaignDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<PreOrderCampaignDto>>> GetCampaignsByProduct(
        Guid productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, _paginationSettings.MaxPageSize);
        if (page < 1) page = 1;

        var query = new GetPreOrderCampaignsByProductQuery(productId, page, pageSize);
        var campaigns = await _mediator.Send(query, cancellationToken);
        return Ok(campaigns);
    }

    /// <summary>
    /// Ön sipariş kampanyasını günceller
    /// </summary>
    [HttpPut("campaigns/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(5, 60)]
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
        var command = new UpdatePreOrderCampaignCommand(
            id,
            dto.Name,
            dto.Description,
            dto.StartDate,
            dto.EndDate,
            dto.ExpectedDeliveryDate,
            dto.MaxQuantity,
            dto.DepositPercentage,
            dto.SpecialPrice);
        var success = await _mediator.Send(command, cancellationToken);

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
    [RateLimit(5, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeactivateCampaign(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeactivatePreOrderCampaignCommand(id);
        var success = await _mediator.Send(command, cancellationToken);

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
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(PreOrderStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PreOrderStatsDto>> GetStats(CancellationToken cancellationToken = default)
    {
        var query = new GetPreOrderStatsQuery();
        var stats = await _mediator.Send(query, cancellationToken);
        return Ok(stats);
    }
}
