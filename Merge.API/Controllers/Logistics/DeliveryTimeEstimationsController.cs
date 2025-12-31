using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.DTOs.Logistics;


namespace Merge.API.Controllers.Logistics;

[ApiController]
[Route("api/logistics/delivery-time")]
public class DeliveryTimeEstimationsController : BaseController
{
    private readonly IDeliveryTimeEstimationService _deliveryTimeEstimationService;

    public DeliveryTimeEstimationsController(IDeliveryTimeEstimationService deliveryTimeEstimationService)
    {
        _deliveryTimeEstimationService = deliveryTimeEstimationService;
    }

    [HttpGet("estimate")]
    [AllowAnonymous]
    public async Task<ActionResult<DeliveryTimeEstimateResultDto>> EstimateDeliveryTime([FromQuery] EstimateDeliveryTimeDto dto)
    {
        var result = await _deliveryTimeEstimationService.EstimateDeliveryTimeAsync(dto);
        return Ok(result);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<DeliveryTimeEstimationDto>>> GetAllEstimations(
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] bool? isActive = null)
    {
        var estimations = await _deliveryTimeEstimationService.GetAllEstimationsAsync(productId, categoryId, warehouseId, isActive);
        return Ok(estimations);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<DeliveryTimeEstimationDto>> GetEstimation(Guid id)
    {
        var estimation = await _deliveryTimeEstimationService.GetEstimationByIdAsync(id);
        if (estimation == null)
        {
            return NotFound();
        }
        return Ok(estimation);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<DeliveryTimeEstimationDto>> CreateEstimation([FromBody] CreateDeliveryTimeEstimationDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var estimation = await _deliveryTimeEstimationService.CreateEstimationAsync(dto);
        return CreatedAtAction(nameof(GetEstimation), new { id = estimation.Id }, estimation);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateEstimation(Guid id, [FromBody] UpdateDeliveryTimeEstimationDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var success = await _deliveryTimeEstimationService.UpdateEstimationAsync(id, dto);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteEstimation(Guid id)
    {
        var success = await _deliveryTimeEstimationService.DeleteEstimationAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }
}

