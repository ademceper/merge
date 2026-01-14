using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Seller;
using Merge.API.Middleware;
using Merge.API.Helpers;
using Merge.Application.Common;
using Merge.Application.Seller.Queries.GetStore;
using Merge.Application.Seller.Queries.GetStoreBySlug;
using Merge.Application.Seller.Queries.GetSellerStores;
using Merge.Application.Seller.Queries.GetPrimaryStore;
using Merge.Application.Seller.Queries.GetStoreStats;
using Merge.Application.Seller.Commands.CreateStore;
using Merge.Application.Seller.Commands.UpdateStore;
using Merge.Application.Seller.Commands.DeleteStore;
using Merge.Application.Seller.Commands.SetPrimaryStore;
using Merge.Application.Seller.Commands.VerifyStore;
using Merge.Application.Seller.Commands.SuspendStore;
using Merge.Domain.Enums;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
namespace Merge.API.Controllers.Seller;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/seller/stores")]
public class StoresController : BaseController
{
    private readonly IMediator _mediator;

    public StoresController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Mağaza detaylarını getirir
    /// </summary>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("{id}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(StoreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<StoreDto>> GetStore(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetStoreQuery(id);
        var store = await _mediator.Send(query, cancellationToken);

        if (store == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetStore", new { version, id }, version);
        links["stats"] = new LinkDto { Href = $"/api/v{version}/seller/stores/{id}/stats", Method = "GET" };
        if (User.Identity?.IsAuthenticated == true && (User.IsInRole("Seller") || User.IsInRole("Admin")))
        {
            links["update"] = new LinkDto { Href = $"/api/v{version}/seller/stores/{id}", Method = "PUT" };
            links["delete"] = new LinkDto { Href = $"/api/v{version}/seller/stores/{id}", Method = "DELETE" };
        }

        return Ok(new { store, _links = links });
    }

    /// <summary>
    /// Slug ile mağaza getirir
    /// </summary>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(StoreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<StoreDto>> GetStoreBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetStoreBySlugQuery(slug);
        var store = await _mediator.Send(query, cancellationToken);

        if (store == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetStoreBySlug", new { version, slug }, version);
        links["store"] = new LinkDto { Href = $"/api/v{version}/seller/stores/{store.Id}", Method = "GET" };
        links["stats"] = new LinkDto { Href = $"/api/v{version}/seller/stores/{store.Id}/stats", Method = "GET" };

        return Ok(new { store, _links = links });
    }

    /// <summary>
    /// Satıcının mağazalarını getirir
    /// </summary>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet("seller/{sellerId}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PagedResult<StoreDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<StoreDto>>> GetSellerStores(
        Guid sellerId,
        [FromQuery] EntityStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
        var query = new GetSellerStoresQuery(sellerId, status, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreatePaginationLinks(Url, "GetSellerStores", page, pageSize, result.TotalPages, new { version, sellerId, status }, version);

        return Ok(new { result.Items, result.TotalCount, result.Page, result.PageSize, result.TotalPages, _links = links });
    }

    /// <summary>
    /// Satıcının ana mağazasını getirir
    /// </summary>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("seller/{sellerId}/primary")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(StoreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<StoreDto>> GetPrimaryStore(
        Guid sellerId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetPrimaryStoreQuery(sellerId);
        var store = await _mediator.Send(query, cancellationToken);

        if (store == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetPrimaryStore", new { version, sellerId }, version);
        links["store"] = new LinkDto { Href = $"/api/v{version}/seller/stores/{store.Id}", Method = "GET" };

        return Ok(new { store, _links = links });
    }

    /// <summary>
    /// Mağaza istatistiklerini getirir
    /// </summary>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("{id}/stats")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(StoreStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<StoreStatsDto>> GetStoreStats(
        Guid id,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetStoreStatsQuery(id, startDate, endDate);
        var stats = await _mediator.Send(query, cancellationToken);

        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetStoreStats", new { version, id, startDate, endDate }, version);
        links["store"] = new LinkDto { Href = $"/api/v{version}/seller/stores/{id}", Method = "GET" };

        return Ok(new { stats, _links = links });
    }

    /// <summary>
    /// Mağaza oluşturur (Seller/Admin)
    /// </summary>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika (Store creation koruması)
    [ProducesResponseType(typeof(StoreDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<StoreDto>> CreateStore(
        [FromBody] CreateStoreDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var sellerId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new CreateStoreCommand(sellerId, dto);
        var store = await _mediator.Send(command, cancellationToken);

        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetStore", new { version, id = store.Id }, version);
        links["update"] = new LinkDto { Href = $"/api/v{version}/seller/stores/{store.Id}", Method = "PUT" };
        links["delete"] = new LinkDto { Href = $"/api/v{version}/seller/stores/{store.Id}", Method = "DELETE" };
        links["stats"] = new LinkDto { Href = $"/api/v{version}/seller/stores/{store.Id}/stats", Method = "GET" };

        return CreatedAtAction(nameof(GetStore), new { version, id = store.Id }, new { store, _links = links });
    }

    /// <summary>
    /// Kullanıcının kendi mağazalarını getirir
    /// </summary>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet("my-stores")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PagedResult<StoreDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<StoreDto>>> GetMyStores(
        [FromQuery] EntityStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var sellerId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
        var query = new GetSellerStoresQuery(sellerId, status, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreatePaginationLinks(Url, "GetMyStores", page, pageSize, result.TotalPages, new { version, status }, version);
        links["create"] = new LinkDto { Href = $"/api/v{version}/seller/stores", Method = "POST" };

        return Ok(new { result.Items, result.TotalCount, result.Page, result.PageSize, result.TotalPages, _links = links });
    }

    /// <summary>
    /// Mağazayı günceller (Seller/Admin)
    /// </summary>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPut("{id}")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateStore(
        Guid id,
        [FromBody] UpdateStoreDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var sellerId = GetUserId();

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi mağazalarını güncelleyebilir
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var getQuery = new GetStoreQuery(id);
        var store = await _mediator.Send(getQuery, cancellationToken);

        if (store == null)
        {
            return NotFound();
        }

        if (store.SellerId != sellerId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new UpdateStoreCommand(id, dto);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Mağazayı siler (Seller/Admin)
    /// </summary>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika (Store deletion koruması)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteStore(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var sellerId = GetUserId();

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi mağazalarını silebilir
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var getQuery = new GetStoreQuery(id);
        var store = await _mediator.Send(getQuery, cancellationToken);

        if (store == null)
        {
            return NotFound();
        }

        if (store.SellerId != sellerId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new DeleteStoreCommand(id);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Mağazayı ana mağaza olarak ayarlar (Seller/Admin)
    /// </summary>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("{id}/set-primary")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SetPrimaryStore(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var sellerId = GetUserId();

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi mağazalarını primary yapabilir
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var getQuery = new GetStoreQuery(id);
        var store = await _mediator.Send(getQuery, cancellationToken);

        if (store == null)
        {
            return NotFound();
        }

        if (store.SellerId != sellerId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new SetPrimaryStoreCommand(sellerId, id);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Mağazayı doğrular (Admin/Manager)
    /// </summary>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("{id}/verify")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> VerifyStore(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new VerifyStoreCommand(id);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Mağazayı askıya alır (Admin/Manager)
    /// </summary>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("{id}/suspend")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SuspendStore(
        Guid id,
        [FromBody] SuspendStoreDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new SuspendStoreCommand(id, dto.Reason);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}
