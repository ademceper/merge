using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Product;
using Merge.Application.DTOs.Product;


namespace Merge.API.Controllers.Product;

[ApiController]
[Route("api/products/comparisons")]
public class ProductComparisonsController : BaseController
{
    private readonly IProductComparisonService _comparisonService;

    public ProductComparisonsController(IProductComparisonService comparisonService)
    {
        _comparisonService = comparisonService;
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ProductComparisonDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProductComparisonDto>> CreateComparison([FromBody] CreateComparisonDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var comparison = await _comparisonService.CreateComparisonAsync(userId, dto);
        return CreatedAtAction(nameof(GetComparison), new { id = comparison.Id }, comparison);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductComparisonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProductComparisonDto>> GetComparison(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var comparison = await _comparisonService.GetComparisonAsync(id);

        if (comparison == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi karşılaştırmalarına erişebilmeli (ShareCode varsa herkes erişebilir)
        if (string.IsNullOrEmpty(comparison.ShareCode) && comparison.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        return Ok(comparison);
    }

    [HttpGet("current")]
    [Authorize]
    [ProducesResponseType(typeof(ProductComparisonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProductComparisonDto>> GetCurrentComparison()
    {
        var userId = GetUserId();
        var comparison = await _comparisonService.GetUserComparisonAsync(userId);
        return Ok(comparison);
    }

    [HttpGet("my-comparisons")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<ProductComparisonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ProductComparisonDto>>> GetMyComparisons([FromQuery] bool savedOnly = false)
    {
        var userId = GetUserId();
        var comparisons = await _comparisonService.GetUserComparisonsAsync(userId, savedOnly);
        return Ok(comparisons);
    }

    [HttpGet("shared/{shareCode}")]
    [ProducesResponseType(typeof(ProductComparisonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductComparisonDto>> GetSharedComparison(string shareCode)
    {
        var comparison = await _comparisonService.GetComparisonByShareCodeAsync(shareCode);

        if (comparison == null)
        {
            return NotFound();
        }

        return Ok(comparison);
    }

    [HttpPost("add")]
    [Authorize]
    [ProducesResponseType(typeof(ProductComparisonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProductComparisonDto>> AddProduct([FromBody] AddToComparisonDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var comparison = await _comparisonService.AddProductToComparisonAsync(userId, dto.ProductId);
        return Ok(comparison);
    }

    [HttpDelete("remove/{productId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveProduct(Guid productId)
    {
        var userId = GetUserId();
        var success = await _comparisonService.RemoveProductFromComparisonAsync(userId, productId);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("save")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SaveComparison([FromBody] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("İsim boş olamaz.");
        }

        var userId = GetUserId();
        var success = await _comparisonService.SaveComparisonAsync(userId, name);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id}/share")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<string>> GenerateShareCode(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi karşılaştırmaları için share code oluşturabilmeli
        var comparison = await _comparisonService.GetComparisonAsync(id);
        if (comparison == null)
        {
            return NotFound();
        }

        if (comparison.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var shareCode = await _comparisonService.GenerateShareCodeAsync(id);
        return Ok(new { shareCode });
    }

    [HttpDelete("clear")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ClearComparison()
    {
        var userId = GetUserId();
        var success = await _comparisonService.ClearComparisonAsync(userId);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteComparison(Guid id)
    {
        var userId = GetUserId();
        var success = await _comparisonService.DeleteComparisonAsync(id, userId);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("{id}/matrix")]
    [ProducesResponseType(typeof(ComparisonMatrixDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ComparisonMatrixDto>> GetComparisonMatrix(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Önce comparison'ı al ve ownership kontrolü yap
        var comparison = await _comparisonService.GetComparisonAsync(id);
        if (comparison == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi karşılaştırmalarına erişebilmeli (ShareCode varsa herkes erişebilir)
        if (string.IsNullOrEmpty(comparison.ShareCode) && comparison.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var matrix = await _comparisonService.GetComparisonMatrixAsync(id);
        return Ok(matrix);
    }
}
