using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Application.Common;
using Merge.Application.Product.Commands.CreateProductComparison;
using Merge.Application.Product.Commands.AddProductToComparison;
using Merge.Application.Product.Commands.RemoveProductFromComparison;
using Merge.Application.Product.Commands.SaveComparison;
using Merge.Application.Product.Commands.GenerateShareCode;
using Merge.Application.Product.Commands.ClearComparison;
using Merge.Application.Product.Commands.DeleteComparison;
using Merge.Application.Product.Queries.GetProductComparisonById;
using Merge.Application.Product.Queries.GetUserComparison;
using Merge.Application.Product.Queries.GetUserComparisons;
using Merge.Application.Product.Queries.GetComparisonByShareCode;
using Merge.Application.Product.Queries.GetComparisonMatrix;
using Merge.API.Middleware;
using Merge.API.Helpers;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
namespace Merge.API.Controllers.Product;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/products/comparisons")]
public class ProductComparisonsController : BaseController
{
    private readonly IMediator _mediator;

    public ProductComparisonsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost]
    [Authorize]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika (Spam koruması)
    [ProducesResponseType(typeof(ProductComparisonDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductComparisonDto>> CreateComparison(
        [FromBody] CreateComparisonDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        var command = new CreateProductComparisonCommand(userId, dto.Name, dto.ProductIds.ToList());
        var comparison = await _mediator.Send(command, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductComparisonLinks(Url, comparison.Id, version);
        
        return CreatedAtAction(nameof(GetComparison), new { id = comparison.Id }, new { comparison, _links = links });
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(ProductComparisonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductComparisonDto>> GetComparison(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetProductComparisonByIdQuery(id);
        var comparison = await _mediator.Send(query, cancellationToken);

        if (comparison == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi karşılaştırmalarına erişebilmeli (ShareCode varsa herkes erişebilir)
        if (string.IsNullOrEmpty(comparison.ShareCode) && comparison.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductComparisonLinks(Url, comparison.Id, version);
        
        return Ok(new { comparison, _links = links });
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("current")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(ProductComparisonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductComparisonDto>> GetCurrentComparison(CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        var query = new GetUserComparisonQuery(userId);
        var comparison = await _mediator.Send(query, cancellationToken);
        
        if (comparison == null)
        {
            return NotFound();
        }
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductComparisonLinks(Url, comparison.Id, version);
        
        return Ok(new { comparison, _links = links });
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet("my-comparisons")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<ProductComparisonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ProductComparisonDto>>> GetMyComparisons(
        [FromQuery] bool savedOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        var query = new GetUserComparisonsQuery(userId, savedOnly, page, pageSize);
        var comparisons = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var paginationLinks = HateoasHelper.CreatePaginationLinks(
            Url,
            "GetMyComparisons",
            page,
            pageSize,
            comparisons.TotalPages,
            new { savedOnly },
            version);
        
        return Ok(new { comparisons, _links = paginationLinks });
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("shared/{shareCode}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(ProductComparisonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductComparisonDto>> GetSharedComparison(
        string shareCode,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetComparisonByShareCodeQuery(shareCode);
        var comparison = await _mediator.Send(query, cancellationToken);

        if (comparison == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductComparisonLinks(Url, comparison.Id, version);
        
        return Ok(new { comparison, _links = links });
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("add")]
    [Authorize]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(ProductComparisonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductComparisonDto>> AddProduct(
        [FromBody] AddToComparisonDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        var command = new AddProductToComparisonCommand(userId, dto.ProductId);
        var comparison = await _mediator.Send(command, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductComparisonLinks(Url, comparison.Id, version);
        
        return Ok(new { comparison, _links = links });
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpDelete("remove/{productId}")]
    [Authorize]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RemoveProduct(Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        var command = new RemoveProductFromComparisonCommand(userId, productId);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("save")]
    [Authorize]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SaveComparison(
        [FromBody] string name,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("İsim boş olamaz.");
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        var command = new SaveComparisonCommand(userId, name);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("{id}/share")]
    [Authorize]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<string>> GenerateShareCode(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi karşılaştırmaları için share code oluşturabilmeli
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var getQuery = new GetProductComparisonByIdQuery(id);
        var comparison = await _mediator.Send(getQuery, cancellationToken);
        if (comparison == null)
        {
            return NotFound();
        }

        if (comparison.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var command = new GenerateShareCodeCommand(id);
        var shareCode = await _mediator.Send(command, cancellationToken);
        return Ok(new { shareCode });
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpDelete("clear")]
    [Authorize]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ClearComparison(CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        var command = new ClearComparisonCommand(userId);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpDelete("{id}")]
    [Authorize]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteComparison(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        var command = new DeleteComparisonCommand(id, userId);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("{id}/matrix")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(ComparisonMatrixDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ComparisonMatrixDto>> GetComparisonMatrix(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Önce comparison'ı al ve ownership kontrolü yap
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var getQuery = new GetProductComparisonByIdQuery(id);
        var comparison = await _mediator.Send(getQuery, cancellationToken);
        if (comparison == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi karşılaştırmalarına erişebilmeli (ShareCode varsa herkes erişebilir)
        if (string.IsNullOrEmpty(comparison.ShareCode) && comparison.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var query = new GetComparisonMatrixQuery(id);
        var matrix = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductComparisonLinks(Url, id, version);
        
        return Ok(new { matrix, _links = links });
    }
}
