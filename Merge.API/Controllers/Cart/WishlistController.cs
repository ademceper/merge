using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Cart.Queries.GetWishlist;
using Merge.Application.Cart.Queries.IsInWishlist;
using Merge.Application.Cart.Commands.AddToWishlist;
using Merge.Application.Cart.Commands.RemoveFromWishlist;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Cart;

// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/cart/wishlist")]
[Authorize]
public class WishlistController : BaseController
{
    private readonly IMediator _mediator;
    private readonly PaginationSettings _paginationSettings;

    public WishlistController(
        IMediator mediator,
        IOptions<PaginationSettings> paginationSettings)
    {
        _mediator = mediator;
        _paginationSettings = paginationSettings.Value;
    }

    /// <summary>
    /// İstek listesini getirir
    /// </summary>
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetWishlist(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - Config'den al
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        var userId = GetUserId();
        var query = new GetWishlistQuery(userId, page, pageSize);
        var products = await _mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// İstek listesine ürün ekler
    /// </summary>
    [HttpPost("{productId}")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AddToWishlist(Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        var command = new AddToWishlistCommand(userId, productId);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return BadRequest();
        }
        return NoContent();
    }

    /// <summary>
    /// İstek listesinden ürün kaldırır
    /// </summary>
    [HttpDelete("{productId}")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RemoveFromWishlist(Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        var command = new RemoveFromWishlistCommand(userId, productId);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Ürünün istek listesinde olup olmadığını kontrol eder
    /// </summary>
    [HttpGet("{productId}/check")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60 istek / dakika (yüksek limit - sık kullanılan)
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<bool>> IsInWishlist(Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        var query = new IsInWishlistQuery(userId, productId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(new { isInWishlist = result });
    }
}

