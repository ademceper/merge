using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Application.Product.Commands.CreateProductBundle;
using Merge.Application.Product.Commands.UpdateProductBundle;
using Merge.Application.Product.Commands.PatchProductBundle;
using Merge.Application.Product.Commands.DeleteProductBundle;
using Merge.Application.Product.Commands.AddProductToBundle;
using Merge.Application.Product.Commands.RemoveProductFromBundle;
using Merge.Application.Product.Queries.GetProductBundleById;
using Merge.Application.Product.Queries.GetAllProductBundles;
using Merge.API.Middleware;
using Merge.API.Helpers;

namespace Merge.API.Controllers.Product;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/products/bundles")]
public class BundlesController(IMediator mediator) : BaseController
{
            [HttpGet]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<ProductBundleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductBundleDto>>> GetActiveBundles(CancellationToken cancellationToken = default)
    {
        var query = new GetAllProductBundlesQuery(ActiveOnly: true);
        var bundles = await mediator.Send(query, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = bundles.Select(b => HateoasHelper.CreateProductBundleLinks(Url, b.Id, version)).ToList();
        return Ok(new { bundles, _links = links });
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<ProductBundleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductBundleDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        var query = new GetAllProductBundlesQuery(ActiveOnly: false);
        var bundles = await mediator.Send(query, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = bundles.Select(b => HateoasHelper.CreateProductBundleLinks(Url, b.Id, version)).ToList();
        return Ok(new { bundles, _links = links });
    }

    [HttpGet("{id}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(ProductBundleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductBundleDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var query = new GetProductBundleByIdQuery(id);
        var bundle = await mediator.Send(query, cancellationToken);
        if (bundle == null)
        {
            return NotFound();
        }
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductBundleLinks(Url, bundle.Id, version);
        return Ok(new { bundle, _links = links });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(ProductBundleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductBundleDto>> Create(
        [FromBody] CreateProductBundleCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        var bundle = await mediator.Send(command, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductBundleLinks(Url, bundle.Id, version);
        return CreatedAtAction(nameof(GetById), new { id = bundle.Id }, new { bundle, _links = links });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(ProductBundleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductBundleDto>> Update(
        Guid id,
        [FromBody] UpdateProductBundleCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        var updatedCommand = command with { Id = id };
        var bundle = await mediator.Send(updatedCommand, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductBundleLinks(Url, bundle.Id, version);
        return Ok(new { bundle, _links = links });
    }

    /// <summary>
    /// Ürün paketini kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(ProductBundleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductBundleDto>> Patch(
        Guid id,
        [FromBody] PatchProductBundleDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        var command = new PatchProductBundleCommand(id, patchDto);
        var bundle = await mediator.Send(command, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductBundleLinks(Url, bundle.Id, version);
        return Ok(new { bundle, _links = links });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var command = new DeleteProductBundleCommand(id);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{bundleId}/products")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AddProduct(
        Guid bundleId,
        [FromBody] AddProductToBundleDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        var command = new AddProductToBundleCommand(bundleId, dto.ProductId, dto.Quantity, dto.SortOrder);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{bundleId}/products/{productId}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RemoveProduct(
        Guid bundleId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var command = new RemoveProductFromBundleCommand(bundleId, productId);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}
