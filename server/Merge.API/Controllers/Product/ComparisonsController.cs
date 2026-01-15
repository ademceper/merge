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

namespace Merge.API.Controllers.Product;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/products/comparisons")]
public class ProductComparisonsController(IMediator mediator) : BaseController
{
            [HttpPost]
    [Authorize]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(ProductComparisonDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductComparisonDto>> CreateComparison(
        [FromBody] CreateComparisonDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        var userId = GetUserId();
        var command = new CreateProductComparisonCommand(userId, dto.Name, dto.ProductIds.ToList());
        var comparison = await mediator.Send(command, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductComparisonLinks(Url, comparison.Id, version);
        return CreatedAtAction(nameof(GetComparison), new { id = comparison.Id }, new { comparison, _links = links });
    }

    [HttpGet("{id}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(ProductComparisonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductComparisonDto>> GetComparison(Guid id, CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var query = new GetProductComparisonByIdQuery(id);
        var comparison = await mediator.Send(query, cancellationToken);

        if (comparison == null)
        {
            return NotFound();
        }
        if (string.IsNullOrEmpty(comparison.ShareCode) && comparison.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductComparisonLinks(Url, comparison.Id, version);
        return Ok(new { comparison, _links = links });
    }

    [HttpGet("current")]
    [Authorize]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(ProductComparisonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductComparisonDto>> GetCurrentComparison(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetUserComparisonQuery(userId);
        var comparison = await mediator.Send(query, cancellationToken);
        
        if (comparison == null)
        {
            return NotFound();
        }
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductComparisonLinks(Url, comparison.Id, version);
        return Ok(new { comparison, _links = links });
    }

    [HttpGet("my-comparisons")]
    [Authorize]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<ProductComparisonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ProductComparisonDto>>> GetMyComparisons(
        [FromQuery] bool savedOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetUserComparisonsQuery(userId, savedOnly, page, pageSize);
        var comparisons = await mediator.Send(query, cancellationToken);
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

    [HttpGet("shared/{shareCode}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(ProductComparisonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductComparisonDto>> GetSharedComparison(
        string shareCode,
        CancellationToken cancellationToken = default)
    {
        var query = new GetComparisonByShareCodeQuery(shareCode);
        var comparison = await mediator.Send(query, cancellationToken);

        if (comparison == null)
        {
            return NotFound();
        }
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductComparisonLinks(Url, comparison.Id, version);
        return Ok(new { comparison, _links = links });
    }

    [HttpPost("add")]
    [Authorize]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(ProductComparisonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductComparisonDto>> AddProduct(
        [FromBody] AddToComparisonDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        var userId = GetUserId();
        var command = new AddProductToComparisonCommand(userId, dto.ProductId);
        var comparison = await mediator.Send(command, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductComparisonLinks(Url, comparison.Id, version);
        return Ok(new { comparison, _links = links });
    }

    [HttpDelete("remove/{productId}")]
    [Authorize]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RemoveProduct(Guid productId, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new RemoveProductFromComparisonCommand(userId, productId);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("save")]
    [Authorize]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SaveComparison(
        [FromBody] string name,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("İsim boş olamaz.");
        }
        var userId = GetUserId();
        var command = new SaveComparisonCommand(userId, name);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/share")]
    [Authorize]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<string>> GenerateShareCode(Guid id, CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var getQuery = new GetProductComparisonByIdQuery(id);
        var comparison = await mediator.Send(getQuery, cancellationToken);
        if (comparison == null)
        {
            return NotFound();
        }
        if (comparison.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        var command = new GenerateShareCodeCommand(id);
        var shareCode = await mediator.Send(command, cancellationToken);
        return Ok(new { shareCode });
    }

    [HttpDelete("clear")]
    [Authorize]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ClearComparison(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new ClearComparisonCommand(userId);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteComparison(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new DeleteComparisonCommand(id, userId);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpGet("{id}/matrix")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(ComparisonMatrixDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ComparisonMatrixDto>> GetComparisonMatrix(Guid id, CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var getQuery = new GetProductComparisonByIdQuery(id);
        var comparison = await mediator.Send(getQuery, cancellationToken);
        if (comparison == null)
        {
            return NotFound();
        }
        if (string.IsNullOrEmpty(comparison.ShareCode) && comparison.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        var query = new GetComparisonMatrixQuery(id);
        var matrix = await mediator.Send(query, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductComparisonLinks(Url, id, version);
        return Ok(new { matrix, _links = links });
    }
}
