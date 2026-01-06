using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Cart;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Cart.Queries.GetCartByUserId;
using Merge.Application.Cart.Queries.GetCartByCartItemId;
using Merge.Application.Cart.Commands.AddItemToCart;
using Merge.Application.Cart.Commands.UpdateCartItem;
using Merge.Application.Cart.Commands.RemoveCartItem;
using Merge.Application.Cart.Commands.ClearCart;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Cart;

// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/cart")]
[Authorize]
public class CartController : BaseController
{
    private readonly IMediator _mediator;
    private readonly PaginationSettings _paginationSettings;

    public CartController(
        IMediator mediator,
        IOptions<PaginationSettings> paginationSettings)
    {
        _mediator = mediator;
        _paginationSettings = paginationSettings.Value;
    }

    /// <summary>
    /// Kullanıcının sepetini getirir
    /// </summary>
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CartDto>> GetCart(CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        var query = new GetCartByUserIdQuery(userId);
        var cart = await _mediator.Send(query, cancellationToken);
        return Ok(cart);
    }

    /// <summary>
    /// Sepete ürün ekler
    /// </summary>
    [HttpPost("items")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(CartItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CartItemDto>> AddItem([FromBody] AddCartItemDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var userId = GetUserId();
        var command = new AddItemToCartCommand(userId, dto.ProductId, dto.Quantity);
        var cartItem = await _mediator.Send(command, cancellationToken);
        return Ok(cartItem);
    }

    /// <summary>
    /// Sepet öğesi miktarını günceller
    /// </summary>
    [HttpPut("items/{cartItemId}")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateItem(Guid cartItemId, [FromBody] UpdateCartItemDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var userId = GetUserId();
        
        // ✅ BOLUM 3.2: IDOR Korumasi - Ownership check (ZORUNLU)
        var cartQuery = new GetCartByCartItemIdQuery(cartItemId);
        var cart = await _mediator.Send(cartQuery, cancellationToken);
        
        if (cart == null)
        {
            return NotFound();
        }

        if (cart.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var command = new UpdateCartItemCommand(cartItemId, dto.Quantity);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Sepetten ürün kaldırır
    /// </summary>
    [HttpDelete("items/{cartItemId}")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RemoveItem(Guid cartItemId, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        
        // ✅ BOLUM 3.2: IDOR Korumasi - Ownership check (ZORUNLU)
        var cartQuery = new GetCartByCartItemIdQuery(cartItemId);
        var cart = await _mediator.Send(cartQuery, cancellationToken);
        
        if (cart == null)
        {
            return NotFound();
        }

        if (cart.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var command = new RemoveCartItemCommand(cartItemId);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Sepeti temizler
    /// </summary>
    [HttpDelete]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ClearCart(CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        var command = new ClearCartCommand(userId);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

