using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Logistics;
using Merge.API.Middleware;
using Merge.Application.Logistics.Queries.EstimateDeliveryTime;
using Merge.Application.Logistics.Queries.GetAllDeliveryTimeEstimations;
using Merge.Application.Logistics.Queries.GetDeliveryTimeEstimationById;
using Merge.Application.Logistics.Commands.CreateDeliveryTimeEstimation;
using Merge.Application.Logistics.Commands.UpdateDeliveryTimeEstimation;
using Merge.Application.Logistics.Commands.DeleteDeliveryTimeEstimation;

namespace Merge.API.Controllers.Logistics;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/logistics/delivery-time")]
public class DeliveryTimeEstimationsController : BaseController
{
    private readonly IMediator _mediator;

    public DeliveryTimeEstimationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Teslimat süresini tahmin eder
    /// </summary>
    [HttpGet("estimate")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(DeliveryTimeEstimateResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<DeliveryTimeEstimateResultDto>> EstimateDeliveryTime(
        [FromQuery] EstimateDeliveryTimeDto dto,
        CancellationToken cancellationToken = default)
    {
        var query = new EstimateDeliveryTimeQuery(
            dto.ProductId,
            dto.CategoryId,
            dto.WarehouseId,
            dto.ShippingProviderId,
            dto.City,
            dto.Country,
            dto.OrderDate);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Tüm teslimat süresi tahminlerini getirir
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<DeliveryTimeEstimationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<DeliveryTimeEstimationDto>>> GetAllEstimations(
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllDeliveryTimeEstimationsQuery(productId, categoryId, warehouseId, isActive);
        var estimations = await _mediator.Send(query, cancellationToken);
        return Ok(estimations);
    }

    /// <summary>
    /// Teslimat süresi tahmini detaylarını getirir
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(DeliveryTimeEstimationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<DeliveryTimeEstimationDto>> GetEstimation(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDeliveryTimeEstimationByIdQuery(id);
        var estimation = await _mediator.Send(query, cancellationToken);
        if (estimation == null)
        {
            return NotFound();
        }
        return Ok(estimation);
    }

    /// <summary>
    /// Yeni teslimat süresi tahmini oluşturur (Admin, Manager)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(DeliveryTimeEstimationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<DeliveryTimeEstimationDto>> CreateEstimation(
        [FromBody] CreateDeliveryTimeEstimationDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new CreateDeliveryTimeEstimationCommand(
            dto.ProductId,
            dto.CategoryId,
            dto.WarehouseId,
            dto.ShippingProviderId,
            dto.City,
            dto.Country,
            dto.MinDays,
            dto.MaxDays,
            dto.AverageDays,
            dto.IsActive,
            dto.Conditions);
        var estimation = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetEstimation), new { id = estimation.Id }, estimation);
    }

    /// <summary>
    /// Teslimat süresi tahminini günceller (Admin, Manager)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateEstimation(
        Guid id,
        [FromBody] UpdateDeliveryTimeEstimationDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new UpdateDeliveryTimeEstimationCommand(
            id,
            dto.MinDays,
            dto.MaxDays,
            dto.AverageDays,
            dto.IsActive,
            dto.Conditions);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Teslimat süresi tahminini siler (Admin, Manager)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteEstimation(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteDeliveryTimeEstimationCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

