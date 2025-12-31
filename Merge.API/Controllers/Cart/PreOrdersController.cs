using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Cart;
using Merge.Application.DTOs.Cart;


namespace Merge.API.Controllers.Cart;

[ApiController]
[Route("api/cart/pre-orders")]
public class PreOrdersController : BaseController
{
    private readonly IPreOrderService _preOrderService;

    public PreOrdersController(IPreOrderService preOrderService)
    {
        _preOrderService = preOrderService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PreOrderDto>> CreatePreOrder([FromBody] CreatePreOrderDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var preOrder = await _preOrderService.CreatePreOrderAsync(userId, dto);
        return CreatedAtAction(nameof(GetPreOrder), new { id = preOrder.Id }, preOrder);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<PreOrderDto>> GetPreOrder(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var preOrder = await _preOrderService.GetPreOrderAsync(id);

        if (preOrder == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: Authorization check - Users can only view their own pre-orders or must be Admin/Manager
        if (preOrder.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        return Ok(preOrder);
    }

    [HttpGet("my-preorders")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<PreOrderDto>>> GetMyPreOrders()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var preOrders = await _preOrderService.GetUserPreOrdersAsync(userId);
        return Ok(preOrders);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> CancelPreOrder(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var success = await _preOrderService.CancelPreOrderAsync(id, userId);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("pay-deposit")]
    [Authorize]
    public async Task<IActionResult> PayDeposit([FromBody] PayPreOrderDepositDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var success = await _preOrderService.PayDepositAsync(userId, dto);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id}/convert")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ConvertToOrder(Guid id)
    {
        var success = await _preOrderService.ConvertToOrderAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id}/notify")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> NotifyAvailable(Guid id)
    {
        await _preOrderService.NotifyPreOrderAvailableAsync(id);
        return NoContent();
    }

    // Campaigns
    [HttpPost("campaigns")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PreOrderCampaignDto>> CreateCampaign([FromBody] CreatePreOrderCampaignDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var campaign = await _preOrderService.CreateCampaignAsync(dto);
        return CreatedAtAction(nameof(GetCampaign), new { id = campaign.Id }, campaign);
    }

    [HttpGet("campaigns/{id}")]
    public async Task<ActionResult<PreOrderCampaignDto>> GetCampaign(Guid id)
    {
        var campaign = await _preOrderService.GetCampaignAsync(id);

        if (campaign == null)
        {
            return NotFound();
        }

        return Ok(campaign);
    }

    [HttpGet("campaigns")]
    public async Task<ActionResult<IEnumerable<PreOrderCampaignDto>>> GetActiveCampaigns()
    {
        var campaigns = await _preOrderService.GetActiveCampaignsAsync();
        return Ok(campaigns);
    }

    [HttpGet("campaigns/product/{productId}")]
    public async Task<ActionResult<IEnumerable<PreOrderCampaignDto>>> GetCampaignsByProduct(Guid productId)
    {
        var campaigns = await _preOrderService.GetCampaignsByProductAsync(productId);
        return Ok(campaigns);
    }

    [HttpPut("campaigns/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateCampaign(Guid id, [FromBody] CreatePreOrderCampaignDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var success = await _preOrderService.UpdateCampaignAsync(id, dto);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("campaigns/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeactivateCampaign(Guid id)
    {
        var success = await _preOrderService.DeactivateCampaignAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PreOrderStatsDto>> GetStats()
    {
        var stats = await _preOrderService.GetPreOrderStatsAsync();
        return Ok(stats);
    }
}
