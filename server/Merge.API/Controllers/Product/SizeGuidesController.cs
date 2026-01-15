using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Application.Product.Commands.CreateSizeGuide;
using Merge.Application.Product.Commands.UpdateSizeGuide;
using Merge.Application.Product.Commands.DeleteSizeGuide;
using Merge.Application.Product.Commands.AssignSizeGuideToProduct;
using Merge.Application.Product.Commands.RemoveSizeGuideFromProduct;
using Merge.Application.Product.Queries.GetSizeGuide;
using Merge.Application.Product.Queries.GetSizeGuidesByCategory;
using Merge.Application.Product.Queries.GetAllSizeGuides;
using Merge.Application.Product.Queries.GetProductSizeGuide;
using Merge.Application.Product.Queries.GetSizeRecommendation;
using Merge.API.Middleware;
using Merge.API.Helpers;

namespace Merge.API.Controllers.Product;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/products/size-guides")]
public class SizeGuidesController(IMediator mediator) : BaseController
{
            [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(SizeGuideDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SizeGuideDto>> CreateSizeGuide(
        [FromBody] CreateSizeGuideDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        var command = new CreateSizeGuideCommand(
            dto.Name,
            dto.Description,
            dto.CategoryId,
            dto.Brand,
            dto.Type,
            dto.MeasurementUnit,
            dto.Entries.ToList());
        var sizeGuide = await mediator.Send(command, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSizeGuideLinks(Url, sizeGuide.Id, version);
        return CreatedAtAction(nameof(GetSizeGuide), new { id = sizeGuide.Id }, new { sizeGuide, _links = links });
    }

    [HttpGet("{id}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(SizeGuideDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SizeGuideDto>> GetSizeGuide(Guid id, CancellationToken cancellationToken = default)
    {
        var query = new GetSizeGuideQuery(id);
        var sizeGuide = await mediator.Send(query, cancellationToken);

        if (sizeGuide == null)
        {
            return NotFound();
        }
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSizeGuideLinks(Url, sizeGuide.Id, version);
        return Ok(new { sizeGuide, _links = links });
    }

    [HttpGet("category/{categoryId}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<SizeGuideDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<SizeGuideDto>>> GetSizeGuidesByCategory(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSizeGuidesByCategoryQuery(categoryId);
        var sizeGuides = await mediator.Send(query, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = sizeGuides.Select(sg => HateoasHelper.CreateSizeGuideLinks(Url, sg.Id, version)).ToList();
        return Ok(new { sizeGuides, _links = links });
    }

    [HttpGet]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<SizeGuideDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<SizeGuideDto>>> GetAllSizeGuides(CancellationToken cancellationToken = default)
    {
        var query = new GetAllSizeGuidesQuery();
        var sizeGuides = await mediator.Send(query, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = sizeGuides.Select(sg => HateoasHelper.CreateSizeGuideLinks(Url, sg.Id, version)).ToList();
        return Ok(new { sizeGuides, _links = links });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateSizeGuide(
        Guid id,
        [FromBody] CreateSizeGuideDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        var command = new UpdateSizeGuideCommand(
            id,
            dto.Name,
            dto.Description,
            dto.CategoryId,
            dto.Brand,
            dto.Type,
            dto.MeasurementUnit,
            dto.Entries.ToList());
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteSizeGuide(Guid id, CancellationToken cancellationToken = default)
    {
        var command = new DeleteSizeGuideCommand(id);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("assign")]
    [Authorize(Roles = "Admin,Manager,Seller")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AssignSizeGuideToProduct(
        [FromBody] AssignSizeGuideDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        var command = new AssignSizeGuideToProductCommand(
            dto.ProductId,
            dto.SizeGuideId,
            dto.CustomNotes,
            dto.FitType,
            dto.FitDescription);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("product/{productId}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(ProductSizeGuideDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductSizeGuideDto>> GetProductSizeGuide(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductSizeGuideQuery(productId);
        var productSizeGuide = await mediator.Send(query, cancellationToken);

        if (productSizeGuide == null)
        {
            return NotFound();
        }
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSizeGuideLinks(Url, productSizeGuide.SizeGuide.Id, version);
        return Ok(new { productSizeGuide, _links = links });
    }

    [HttpDelete("product/{productId}")]
    [Authorize(Roles = "Admin,Manager,Seller")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RemoveSizeGuideFromProduct(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var command = new RemoveSizeGuideFromProductCommand(productId);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("recommend")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(SizeRecommendationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SizeRecommendationDto>> GetSizeRecommendation(
        [FromBody] GetSizeRecommendationDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        var query = new GetSizeRecommendationQuery(
            dto.ProductId,
            dto.Height,
            dto.Weight,
            dto.Chest,
            dto.Waist);
        var recommendation = await mediator.Send(query, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetSizeRecommendation", new { dto.ProductId }, version);
        return Ok(new { recommendation, _links = links });
    }
}
