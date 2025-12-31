using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.B2B;
using Merge.Application.DTOs.B2B;

namespace Merge.API.Controllers.B2B;

[ApiController]
[Route("api/b2b")]
[Authorize]
public class B2BController : BaseController
{
    private readonly IB2BService _b2bService;

    public B2BController(IB2BService b2bService)
    {
        _b2bService = b2bService;
    }

    // B2B Users
    [HttpPost("users")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<B2BUserDto>> CreateB2BUser([FromBody] CreateB2BUserDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var b2bUser = await _b2bService.CreateB2BUserAsync(dto);
        return CreatedAtAction(nameof(GetB2BUser), new { id = b2bUser.Id }, b2bUser);
    }

    [HttpGet("users/{id}")]
    public async Task<ActionResult<B2BUserDto>> GetB2BUser(Guid id)
    {
        var userId = GetUserId();
        var b2bUser = await _b2bService.GetB2BUserByIdAsync(id);
        if (b2bUser == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: Authorization check - Users can only view their own B2B profile or must be Admin/Manager
        if (b2bUser.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        return Ok(b2bUser);
    }

    [HttpGet("users/my-profile")]
    public async Task<ActionResult<B2BUserDto>> GetMyB2BProfile()
    {
        var userId = GetUserId();
        var b2bUser = await _b2bService.GetB2BUserByUserIdAsync(userId);
        if (b2bUser == null)
        {
            return NotFound();
        }
        return Ok(b2bUser);
    }

    [HttpGet("organizations/{organizationId}/users")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<B2BUserDto>>> GetOrganizationB2BUsers(Guid organizationId, [FromQuery] string? status = null)
    {
        var users = await _b2bService.GetOrganizationB2BUsersAsync(organizationId, status);
        return Ok(users);
    }

    [HttpPut("users/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateB2BUser(Guid id, [FromBody] UpdateB2BUserDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var success = await _b2bService.UpdateB2BUserAsync(id, dto);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("users/{id}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ApproveB2BUser(Guid id)
    {
        var approvedBy = GetUserId();
        var success = await _b2bService.ApproveB2BUserAsync(id, approvedBy);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    // Wholesale Prices
    [HttpPost("wholesale-prices")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<WholesalePriceDto>> CreateWholesalePrice([FromBody] CreateWholesalePriceDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var price = await _b2bService.CreateWholesalePriceAsync(dto);
        return CreatedAtAction(nameof(GetProductWholesalePrices), new { productId = price.ProductId }, price);
    }

    [HttpGet("products/{productId}/wholesale-prices")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<WholesalePriceDto>>> GetProductWholesalePrices(Guid productId, [FromQuery] Guid? organizationId = null)
    {
        var prices = await _b2bService.GetProductWholesalePricesAsync(productId, organizationId);
        return Ok(prices);
    }

    [HttpGet("products/{productId}/wholesale-price")]
    [AllowAnonymous]
    public async Task<ActionResult<decimal?>> GetWholesalePrice(Guid productId, [FromQuery] int quantity, [FromQuery] Guid? organizationId = null)
    {
        var price = await _b2bService.GetWholesalePriceAsync(productId, quantity, organizationId);
        return Ok(new { productId, quantity, organizationId, price });
    }

    // Credit Terms
    [HttpPost("credit-terms")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<CreditTermDto>> CreateCreditTerm([FromBody] CreateCreditTermDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var creditTerm = await _b2bService.CreateCreditTermAsync(dto);
        return CreatedAtAction(nameof(GetOrganizationCreditTerms), new { organizationId = creditTerm.OrganizationId }, creditTerm);
    }

    [HttpGet("organizations/{organizationId}/credit-terms")]
    public async Task<ActionResult<IEnumerable<CreditTermDto>>> GetOrganizationCreditTerms(Guid organizationId, [FromQuery] bool? isActive = null)
    {
        var userId = GetUserId();
        var b2bUser = await _b2bService.GetB2BUserByUserIdAsync(userId);
        
        // ✅ SECURITY: Authorization check - Users can only view credit terms for their own organization or must be Admin/Manager
        if (b2bUser == null || (b2bUser.OrganizationId != organizationId && !User.IsInRole("Admin") && !User.IsInRole("Manager")))
        {
            return Forbid();
        }

        var creditTerms = await _b2bService.GetOrganizationCreditTermsAsync(organizationId, isActive);
        return Ok(creditTerms);
    }

    // Purchase Orders
    [HttpPost("purchase-orders")]
    public async Task<ActionResult<PurchaseOrderDto>> CreatePurchaseOrder([FromBody] CreatePurchaseOrderDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var b2bUser = await _b2bService.GetB2BUserByUserIdAsync(userId);
        if (b2bUser == null)
        {
            return BadRequest("B2B kullanıcı profili bulunamadı.");
        }

        var po = await _b2bService.CreatePurchaseOrderAsync(b2bUser.Id, dto);
        return CreatedAtAction(nameof(GetPurchaseOrder), new { id = po.Id }, po);
    }

    [HttpGet("purchase-orders/{id}")]
    public async Task<ActionResult<PurchaseOrderDto>> GetPurchaseOrder(Guid id)
    {
        var userId = GetUserId();
        var po = await _b2bService.GetPurchaseOrderByIdAsync(id);
        if (po == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: Authorization check - Users can only view their own purchase orders or must be Admin/Manager
        var b2bUser = await _b2bService.GetB2BUserByUserIdAsync(userId);
        if (b2bUser == null || (po.B2BUserId != b2bUser.Id && !User.IsInRole("Admin") && !User.IsInRole("Manager")))
        {
            return Forbid();
        }

        return Ok(po);
    }

    [HttpGet("purchase-orders/po-number/{poNumber}")]
    public async Task<ActionResult<PurchaseOrderDto>> GetPurchaseOrderByPONumber(string poNumber)
    {
        var userId = GetUserId();
        var po = await _b2bService.GetPurchaseOrderByPONumberAsync(poNumber);
        if (po == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: Authorization check - Users can only view their own purchase orders or must be Admin/Manager
        var b2bUser = await _b2bService.GetB2BUserByUserIdAsync(userId);
        if (b2bUser == null || (po.B2BUserId != b2bUser.Id && !User.IsInRole("Admin") && !User.IsInRole("Manager")))
        {
            return Forbid();
        }

        return Ok(po);
    }

    [HttpGet("organizations/{organizationId}/purchase-orders")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<PurchaseOrderDto>>> GetOrganizationPurchaseOrders(Guid organizationId, [FromQuery] string? status = null)
    {
        var pos = await _b2bService.GetOrganizationPurchaseOrdersAsync(organizationId, status);
        return Ok(pos);
    }

    [HttpGet("purchase-orders/my-orders")]
    public async Task<ActionResult<IEnumerable<PurchaseOrderDto>>> GetMyPurchaseOrders([FromQuery] string? status = null)
    {
        var userId = GetUserId();
        var b2bUser = await _b2bService.GetB2BUserByUserIdAsync(userId);
        if (b2bUser == null)
        {
            return BadRequest();
        }

        var pos = await _b2bService.GetB2BUserPurchaseOrdersAsync(b2bUser.Id, status);
        return Ok(pos);
    }

    [HttpPost("purchase-orders/{id}/submit")]
    public async Task<IActionResult> SubmitPurchaseOrder(Guid id)
    {
        var userId = GetUserId();
        var po = await _b2bService.GetPurchaseOrderByIdAsync(id);
        if (po == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: Authorization check - Users can only submit their own purchase orders
        var b2bUser = await _b2bService.GetB2BUserByUserIdAsync(userId);
        if (b2bUser == null || po.B2BUserId != b2bUser.Id)
        {
            return Forbid();
        }

        var success = await _b2bService.SubmitPurchaseOrderAsync(id);
        if (!success)
        {
            return BadRequest("Sipariş gönderilemedi.");
        }
        return NoContent();
    }

    [HttpPost("purchase-orders/{id}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ApprovePurchaseOrder(Guid id)
    {
        var approvedBy = GetUserId();
        var success = await _b2bService.ApprovePurchaseOrderAsync(id, approvedBy);
        if (!success)
        {
            return BadRequest("Sipariş onaylanamadı.");
        }
        return NoContent();
    }

    [HttpPost("purchase-orders/{id}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RejectPurchaseOrder(Guid id, [FromBody] RejectPODto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var success = await _b2bService.RejectPurchaseOrderAsync(id, dto.Reason);
        if (!success)
        {
            return BadRequest("Sipariş reddedilemedi.");
        }
        return NoContent();
    }

    [HttpPost("purchase-orders/{id}/cancel")]
    public async Task<IActionResult> CancelPurchaseOrder(Guid id)
    {
        var userId = GetUserId();
        var po = await _b2bService.GetPurchaseOrderByIdAsync(id);
        if (po == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: Authorization check - Users can only cancel their own purchase orders
        var b2bUser = await _b2bService.GetB2BUserByUserIdAsync(userId);
        if (b2bUser == null || po.B2BUserId != b2bUser.Id)
        {
            return Forbid();
        }

        var success = await _b2bService.CancelPurchaseOrderAsync(id);
        if (!success)
        {
            return BadRequest("Sipariş iptal edilemedi.");
        }
        return NoContent();
    }

    // Volume Discounts
    [HttpPost("volume-discounts")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<VolumeDiscountDto>> CreateVolumeDiscount([FromBody] CreateVolumeDiscountDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var discount = await _b2bService.CreateVolumeDiscountAsync(dto);
        return CreatedAtAction(nameof(GetVolumeDiscounts), new { productId = discount.ProductId }, discount);
    }

    [HttpGet("volume-discounts")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<VolumeDiscountDto>>> GetVolumeDiscounts(
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? organizationId = null)
    {
        var discounts = await _b2bService.GetVolumeDiscountsAsync(productId, categoryId, organizationId);
        return Ok(discounts);
    }
}

