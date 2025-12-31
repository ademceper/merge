using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Services;
using Merge.Application.Interfaces.LiveCommerce;
using Merge.Application.DTOs.LiveCommerce;

namespace Merge.API.Controllers.LiveCommerce;

[ApiController]
[Route("api/live-commerce")]
[Authorize]
public class LiveCommerceController : BaseController
{
    private readonly ILiveCommerceService _liveCommerceService;
        public LiveCommerceController(ILiveCommerceService liveCommerceService)
    {
        _liveCommerceService = liveCommerceService;
            }

    [HttpPost("streams")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<ActionResult<LiveStreamDto>> CreateStream([FromBody] CreateLiveStreamDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var stream = await _liveCommerceService.CreateStreamAsync(dto);
        return CreatedAtAction(nameof(GetStream), new { id = stream.Id }, stream);
    }

    [HttpGet("streams/{id}")]
    public async Task<ActionResult<LiveStreamDto>> GetStream(Guid id)
    {
        var stream = await _liveCommerceService.GetStreamAsync(id);
        if (stream == null)
        {
            return NotFound();
        }
        return Ok(stream);
    }

    [HttpGet("streams/active")]
    public async Task<ActionResult<IEnumerable<LiveStreamDto>>> GetActiveStreams()
    {
        var streams = await _liveCommerceService.GetActiveStreamsAsync();
        return Ok(streams);
    }

    [HttpGet("streams/seller/{sellerId}")]
    public async Task<ActionResult<IEnumerable<LiveStreamDto>>> GetStreamsBySeller(Guid sellerId)
    {
        var streams = await _liveCommerceService.GetStreamsBySellerAsync(sellerId);
        return Ok(streams);
    }

    [HttpPost("streams/{id}/start")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<IActionResult> StartStream(Guid id)
    {
        var result = await _liveCommerceService.StartStreamAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("streams/{id}/end")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<IActionResult> EndStream(Guid id)
    {
        var result = await _liveCommerceService.EndStreamAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("streams/{streamId}/products/{productId}")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<ActionResult<LiveStreamDto>> AddProduct(Guid streamId, Guid productId, [FromBody] AddProductToStreamDto? dto = null)
    {
        if (dto != null)
        {
            var validationResult = ValidateModelState();
            if (validationResult != null) return validationResult;
        }

        var stream = await _liveCommerceService.AddProductToStreamAsync(streamId, productId, dto);
        if (stream == null)
        {
            return NotFound();
        }
        return Ok(stream);
    }

    [HttpPost("streams/{streamId}/products/{productId}/showcase")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<IActionResult> ShowcaseProduct(Guid streamId, Guid productId)
    {
        var result = await _liveCommerceService.ShowcaseProductAsync(streamId, productId);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("streams/{streamId}/join")]
    public async Task<IActionResult> JoinStream(Guid streamId, [FromBody] JoinStreamDto? dto = null)
    {
        if (dto != null)
        {
            var validationResult = ValidateModelState();
            if (validationResult != null) return validationResult;
        }

        var userId = GetUserIdOrNull();
        var result = await _liveCommerceService.JoinStreamAsync(streamId, userId, dto?.GuestId);
        if (!result)
        {
            return BadRequest("Yayına katılamadı.");
        }
        return NoContent();
    }

    [HttpPost("streams/{streamId}/leave")]
    public async Task<IActionResult> LeaveStream(Guid streamId, [FromBody] LeaveStreamDto? dto = null)
    {
        if (dto != null)
        {
            var validationResult = ValidateModelState();
            if (validationResult != null) return validationResult;
        }

        var userId = GetUserIdOrNull();
        var result = await _liveCommerceService.LeaveStreamAsync(streamId, userId, dto?.GuestId);
        if (!result)
        {
            return BadRequest("Yayından ayrılamadı.");
        }
        return NoContent();
    }

    [HttpPost("streams/{streamId}/orders/{orderId}")]
    [Authorize]
    public async Task<ActionResult<LiveStreamOrderDto>> CreateOrderFromStream(Guid streamId, Guid orderId, [FromQuery] Guid? productId = null)
    {
        var streamOrder = await _liveCommerceService.CreateOrderFromStreamAsync(streamId, orderId, productId);
        return Ok(streamOrder);
    }

    [HttpGet("streams/{id}/stats")]
    public async Task<ActionResult<LiveStreamStatsDto>> GetStreamStats(Guid id)
    {
        var stats = await _liveCommerceService.GetStreamStatsAsync(id);
        return Ok(stats);
    }
}

