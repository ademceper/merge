using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Marketing.Queries.GetActiveFlashSales;
using Merge.Application.Marketing.Queries.GetAllFlashSales;
using Merge.Application.Marketing.Queries.GetFlashSaleById;
using Merge.Application.Marketing.Commands.CreateFlashSale;
using Merge.Application.Marketing.Commands.UpdateFlashSale;
using Merge.Application.Marketing.Commands.PatchFlashSale;
using Merge.Application.Marketing.Commands.DeleteFlashSale;
using Merge.Application.Marketing.Commands.AddProductToFlashSale;
using Merge.Application.Marketing.Commands.RemoveProductFromFlashSale;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.API.Controllers.Marketing;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/marketing/flash-sales")]
public class FlashSalesController(
    IMediator mediator,
    IOptions<MarketingSettings> marketingSettings) : BaseController
{
    private readonly MarketingSettings _marketingSettings = marketingSettings.Value;

    [HttpGet]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<FlashSaleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<FlashSaleDto>>> GetActiveSales(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _marketingSettings.MaxPageSize) pageSize = _marketingSettings.MaxPageSize;

        var query = new GetActiveFlashSalesQuery(PageNumber: page, PageSize: pageSize);
        var sales = await mediator.Send(query, cancellationToken);
        return Ok(sales);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<FlashSaleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<FlashSaleDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _marketingSettings.MaxPageSize) pageSize = _marketingSettings.MaxPageSize;

        var query = new GetAllFlashSalesQuery(PageNumber: page, PageSize: pageSize);
        var sales = await mediator.Send(query, cancellationToken);
        return Ok(sales);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(FlashSaleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FlashSaleDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFlashSaleByIdQuery(id);
        var sale = await mediator.Send(query, cancellationToken);
        
        if (sale == null)
        {
            return NotFound();
        }
        
        return Ok(sale);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(FlashSaleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FlashSaleDto>> Create(
        [FromBody] CreateFlashSaleDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateFlashSaleCommand(
            dto.Title,
            dto.Description,
            dto.StartDate,
            dto.EndDate,
            dto.BannerImageUrl);
        
        var sale = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = sale.Id }, sale);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(FlashSaleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FlashSaleDto>> Update(
        Guid id,
        [FromBody] UpdateFlashSaleDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateFlashSaleCommand(
            id,
            dto.Title,
            dto.Description,
            dto.StartDate,
            dto.EndDate,
            dto.IsActive,
            dto.BannerImageUrl);
        
        var sale = await mediator.Send(command, cancellationToken);
        return Ok(sale);
    }

    /// <summary>
    /// Flash sale'i kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(FlashSaleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FlashSaleDto>> Patch(
        Guid id,
        [FromBody] PatchFlashSaleDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var command = new PatchFlashSaleCommand(id, patchDto);
        var sale = await mediator.Send(command, cancellationToken);
        return Ok(sale);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteFlashSaleCommand(id);
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }

    [HttpPost("{flashSaleId}/products")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AddProduct(
        Guid flashSaleId,
        [FromBody] AddProductToSaleDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new AddProductToFlashSaleCommand(
            flashSaleId,
            dto.ProductId,
            dto.SalePrice,
            dto.StockLimit,
            dto.SortOrder);
        
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{flashSaleId}/products/{productId}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RemoveProduct(
        Guid flashSaleId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var command = new RemoveProductFromFlashSaleCommand(flashSaleId, productId);
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}
