using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.B2B;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.B2B.Commands.CreateB2BUser;
using Merge.Application.B2B.Commands.ApproveB2BUser;
using Merge.Application.B2B.Commands.UpdateB2BUser;
using Merge.Application.B2B.Commands.DeleteB2BUser;
using Merge.Application.B2B.Commands.CreatePurchaseOrder;
using Merge.Application.B2B.Commands.SubmitPurchaseOrder;
using Merge.Application.B2B.Commands.ApprovePurchaseOrder;
using Merge.Application.B2B.Commands.RejectPurchaseOrder;
using Merge.Application.B2B.Commands.CancelPurchaseOrder;
using Merge.Application.B2B.Commands.CreateWholesalePrice;
using Merge.Application.B2B.Commands.UpdateWholesalePrice;
using Merge.Application.B2B.Commands.DeleteWholesalePrice;
using Merge.Application.B2B.Commands.CreateCreditTerm;
using Merge.Application.B2B.Commands.UpdateCreditTerm;
using Merge.Application.B2B.Commands.DeleteCreditTerm;
using Merge.Application.B2B.Commands.CreateVolumeDiscount;
using Merge.Application.B2B.Commands.UpdateVolumeDiscount;
using Merge.Application.B2B.Commands.DeleteVolumeDiscount;
using Merge.Application.B2B.Queries.GetB2BUserById;
using Merge.Application.B2B.Queries.GetB2BUserByUserId;
using Merge.Application.B2B.Queries.GetOrganizationB2BUsers;
using Merge.Application.B2B.Queries.GetPurchaseOrderById;
using Merge.Application.B2B.Queries.GetPurchaseOrderByPONumber;
using Merge.Application.B2B.Queries.GetOrganizationPurchaseOrders;
using Merge.Application.B2B.Queries.GetB2BUserPurchaseOrders;
using Merge.Application.B2B.Queries.GetProductWholesalePrices;
using Merge.Application.B2B.Queries.GetWholesalePrice;
using Merge.Application.B2B.Queries.GetCreditTermById;
using Merge.Application.B2B.Queries.GetOrganizationCreditTerms;
using Merge.Application.B2B.Queries.GetVolumeDiscounts;
using Merge.Application.B2B.Queries.CalculateVolumeDiscount;
using Merge.API.Middleware;

namespace Merge.API.Controllers.B2B;

// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/b2b")]
[Authorize]
public class B2BController : BaseController
{
    private readonly IMediator _mediator;
    private readonly PaginationSettings _paginationSettings;

    public B2BController(
        IMediator mediator,
        IOptions<PaginationSettings> paginationSettings)
    {
        _mediator = mediator;
        _paginationSettings = paginationSettings.Value;
    }

    // B2B Users
    /// <summary>
    /// Yeni B2B kullanıcısı oluşturur
    /// </summary>
    [HttpPost("users")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika (Spam koruması)
    [ProducesResponseType(typeof(B2BUserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<B2BUserDto>> CreateB2BUser(
        [FromBody] CreateB2BUserDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var command = new CreateB2BUserCommand(
            dto.UserId,
            dto.OrganizationId,
            dto.EmployeeId,
            dto.Department,
            dto.JobTitle,
            dto.CreditLimit,
            dto.Settings);
        
        var b2bUser = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetB2BUser), new { id = b2bUser.Id }, b2bUser);
    }

    /// <summary>
    /// B2B kullanıcı detaylarını getirir
    /// </summary>
    [HttpGet("users/{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(B2BUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<B2BUserDto>> GetB2BUser(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetB2BUserByIdQuery(id);
        var b2bUser = await _mediator.Send(query, cancellationToken);
        
        if (b2bUser == null)
        {
            return NotFound();
        }

        var userId = GetUserId();
        // ✅ BOLUM 3.2: IDOR Korumasi - Ownership check (ZORUNLU)
        if (b2bUser.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        return Ok(b2bUser);
    }

    /// <summary>
    /// Kullanıcının kendi B2B profilini getirir
    /// </summary>
    [HttpGet("users/my-profile")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(B2BUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<B2BUserDto>> GetMyB2BProfile(CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        var query = new GetB2BUserByUserIdQuery(userId);
        var b2bUser = await _mediator.Send(query, cancellationToken);
        
        if (b2bUser == null)
        {
            return NotFound();
        }
        return Ok(b2bUser);
    }

    /// <summary>
    /// Organizasyonun B2B kullanıcılarını listeler
    /// </summary>
    [HttpGet("organizations/{organizationId}/users")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<B2BUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<B2BUserDto>>> GetOrganizationB2BUsers(
        Guid organizationId,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - Config'den al
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        var query = new GetOrganizationB2BUsersQuery(organizationId, status, page, pageSize);
        var users = await _mediator.Send(query, cancellationToken);
        return Ok(users);
    }

    /// <summary>
    /// B2B kullanıcı bilgilerini günceller
    /// </summary>
    [HttpPut("users/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateB2BUser(Guid id, [FromBody] UpdateB2BUserDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz

        var userId = GetUserId();
        var b2bUserQuery = new GetB2BUserByIdQuery(id);
        var b2bUser = await _mediator.Send(b2bUserQuery, cancellationToken);
        
        if (b2bUser == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 3.2: IDOR Korumasi - Ownership check (ZORUNLU)
        // Admin/Manager rolü varsa tüm B2B kullanıcıları güncelleyebilir
        // Normal kullanıcı sadece kendi B2B profilini güncelleyebilir
        if (!User.IsInRole("Admin") && !User.IsInRole("Manager") && b2bUser.UserId != userId)
        {
            return Forbid();
        }

        var command = new UpdateB2BUserCommand(id, dto);
        var success = await _mediator.Send(command, cancellationToken);
        
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// B2B kullanıcıyı onaylar
    /// </summary>
    [HttpPost("users/{id}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ApproveB2BUser(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var approvedBy = GetUserId();
        var command = new ApproveB2BUserCommand(id, approvedBy);
        var success = await _mediator.Send(command, cancellationToken);
        
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    // Wholesale Prices
    /// <summary>
    /// Yeni toptan satış fiyatı oluşturur
    /// </summary>
    [HttpPost("wholesale-prices")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20/dakika
    [ProducesResponseType(typeof(WholesalePriceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<WholesalePriceDto>> CreateWholesalePrice([FromBody] CreateWholesalePriceDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz

        var command = new CreateWholesalePriceCommand(dto);
        var price = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetProductWholesalePrices), new { productId = price.ProductId }, price);
    }

    /// <summary>
    /// Ürün için toptan satış fiyatlarını listeler
    /// </summary>
    [HttpGet("products/{productId}/wholesale-prices")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<WholesalePriceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<WholesalePriceDto>>> GetProductWholesalePrices(
        Guid productId,
        [FromQuery] Guid? organizationId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - Config'den al
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        var query = new GetProductWholesalePricesQuery(productId, organizationId, page, pageSize);
        var prices = await _mediator.Send(query, cancellationToken);
        return Ok(prices);
    }

    /// <summary>
    /// Belirli miktar için toptan satış fiyatını getirir
    /// </summary>
    [HttpGet("products/{productId}/wholesale-price")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(WholesalePriceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<WholesalePriceResponseDto>> GetWholesalePrice(
        Guid productId,
        [FromQuery] int quantity,
        [FromQuery] Guid? organizationId = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel validation gereksiz
        var query = new GetWholesalePriceQuery(productId, quantity, organizationId);
        var price = await _mediator.Send(query, cancellationToken);
        
        return Ok(new WholesalePriceResponseDto
        {
            ProductId = productId,
            Quantity = quantity,
            OrganizationId = organizationId,
            Price = price
        });
    }

    // Credit Terms
    /// <summary>
    /// Yeni kredi koşulu oluşturur
    /// </summary>
    [HttpPost("credit-terms")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20/dakika
    [ProducesResponseType(typeof(CreditTermDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CreditTermDto>> CreateCreditTerm([FromBody] CreateCreditTermDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz

        var command = new CreateCreditTermCommand(dto);
        var creditTerm = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetOrganizationCreditTerms), new { organizationId = creditTerm.OrganizationId }, creditTerm);
    }

    /// <summary>
    /// Organizasyonun kredi koşullarını listeler
    /// </summary>
    [HttpGet("organizations/{organizationId}/credit-terms")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<CreditTermDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<CreditTermDto>>> GetOrganizationCreditTerms(
        Guid organizationId,
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - Config'den al
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        var userId = GetUserId();
        var b2bUserQuery = new GetB2BUserByUserIdQuery(userId);
        var b2bUser = await _mediator.Send(b2bUserQuery, cancellationToken);
        
        // ✅ SECURITY: Authorization check - Users can only view credit terms for their own organization or must be Admin/Manager
        if (b2bUser == null || (b2bUser.OrganizationId != organizationId && !User.IsInRole("Admin") && !User.IsInRole("Manager")))
        {
            return Forbid();
        }

        var query = new GetOrganizationCreditTermsQuery(organizationId, isActive, page, pageSize);
        var creditTerms = await _mediator.Send(query, cancellationToken);
        return Ok(creditTerms);
    }

    // Purchase Orders
    /// <summary>
    /// Yeni satın alma siparişi oluşturur
    /// </summary>
    [HttpPost("purchase-orders")]
    [RateLimit(10, 3600)] // ✅ BOLUM 3.3: Rate Limiting - 10/saat (Fraud koruması)
    [ProducesResponseType(typeof(PurchaseOrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PurchaseOrderDto>> CreatePurchaseOrder([FromBody] CreatePurchaseOrderDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz

        var userId = GetUserId();
        var b2bUserQuery = new GetB2BUserByUserIdQuery(userId);
        var b2bUser = await _mediator.Send(b2bUserQuery, cancellationToken);
        
        if (b2bUser == null)
        {
            return BadRequest("B2B kullanıcı profili bulunamadı.");
        }

        var command = new CreatePurchaseOrderCommand(b2bUser.Id, dto);
        var po = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetPurchaseOrder), new { id = po.Id }, po);
    }

    /// <summary>
    /// Satın alma siparişi detaylarını getirir
    /// </summary>
    [HttpGet("purchase-orders/{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PurchaseOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PurchaseOrderDto>> GetPurchaseOrder(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        var poQuery = new GetPurchaseOrderByIdQuery(id);
        var po = await _mediator.Send(poQuery, cancellationToken);
        
        if (po == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: Authorization check - Users can only view their own purchase orders or must be Admin/Manager
        var b2bUserQuery = new GetB2BUserByUserIdQuery(userId);
        var b2bUser = await _mediator.Send(b2bUserQuery, cancellationToken);
        
        if (b2bUser == null || (po.B2BUserId != b2bUser.Id && !User.IsInRole("Admin") && !User.IsInRole("Manager")))
        {
            return Forbid();
        }

        return Ok(po);
    }

    /// <summary>
    /// PO numarasına göre satın alma siparişi getirir
    /// </summary>
    [HttpGet("purchase-orders/po-number/{poNumber}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PurchaseOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PurchaseOrderDto>> GetPurchaseOrderByPONumber(string poNumber, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        var poQuery = new GetPurchaseOrderByPONumberQuery(poNumber);
        var po = await _mediator.Send(poQuery, cancellationToken);
        
        if (po == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: Authorization check - Users can only view their own purchase orders or must be Admin/Manager
        var b2bUserQuery = new GetB2BUserByUserIdQuery(userId);
        var b2bUser = await _mediator.Send(b2bUserQuery, cancellationToken);
        
        if (b2bUser == null || (po.B2BUserId != b2bUser.Id && !User.IsInRole("Admin") && !User.IsInRole("Manager")))
        {
            return Forbid();
        }

        return Ok(po);
    }

    /// <summary>
    /// Organizasyonun satın alma siparişlerini listeler
    /// </summary>
    [HttpGet("organizations/{organizationId}/purchase-orders")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<PurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<PurchaseOrderDto>>> GetOrganizationPurchaseOrders(
        Guid organizationId,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - Config'den al
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        var query = new GetOrganizationPurchaseOrdersQuery(organizationId, status, page, pageSize);
        var pos = await _mediator.Send(query, cancellationToken);
        return Ok(pos);
    }

    /// <summary>
    /// Kullanıcının satın alma siparişlerini listeler
    /// </summary>
    [HttpGet("purchase-orders/my-orders")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<PurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<PurchaseOrderDto>>> GetMyPurchaseOrders(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - Config'den al
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        var userId = GetUserId();
        var b2bUserQuery = new GetB2BUserByUserIdQuery(userId);
        var b2bUser = await _mediator.Send(b2bUserQuery, cancellationToken);
        
        if (b2bUser == null)
        {
            return BadRequest();
        }

        var query = new GetB2BUserPurchaseOrdersQuery(b2bUser.Id, status, page, pageSize);
        var pos = await _mediator.Send(query, cancellationToken);
        return Ok(pos);
    }

    /// <summary>
    /// Satın alma siparişini gönderir
    /// </summary>
    [HttpPost("purchase-orders/{id}/submit")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SubmitPurchaseOrder(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        var poQuery = new GetPurchaseOrderByIdQuery(id);
        var po = await _mediator.Send(poQuery, cancellationToken);
        
        if (po == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: Authorization check - Users can only submit their own purchase orders
        var b2bUserQuery = new GetB2BUserByUserIdQuery(userId);
        var b2bUser = await _mediator.Send(b2bUserQuery, cancellationToken);
        
        if (b2bUser == null || po.B2BUserId != b2bUser.Id)
        {
            return Forbid();
        }

        var command = new SubmitPurchaseOrderCommand(id);
        var success = await _mediator.Send(command, cancellationToken);
        
        if (!success)
        {
            return BadRequest("Sipariş gönderilemedi.");
        }
        return NoContent();
    }

    /// <summary>
    /// Satın alma siparişini onaylar
    /// </summary>
    [HttpPost("purchase-orders/{id}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ApprovePurchaseOrder(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var approvedBy = GetUserId();
        var command = new ApprovePurchaseOrderCommand(id, approvedBy);
        var success = await _mediator.Send(command, cancellationToken);
        
        if (!success)
        {
            return BadRequest("Sipariş onaylanamadı.");
        }
        return NoContent();
    }

    /// <summary>
    /// Satın alma siparişini reddeder
    /// </summary>
    [HttpPost("purchase-orders/{id}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RejectPurchaseOrder(Guid id, [FromBody] RejectPODto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz

        var command = new RejectPurchaseOrderCommand(id, dto.Reason);
        var success = await _mediator.Send(command, cancellationToken);
        
        if (!success)
        {
            return BadRequest("Sipariş reddedilemedi.");
        }
        return NoContent();
    }

    /// <summary>
    /// Satın alma siparişini iptal eder
    /// </summary>
    [HttpPost("purchase-orders/{id}/cancel")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CancelPurchaseOrder(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        var poQuery = new GetPurchaseOrderByIdQuery(id);
        var po = await _mediator.Send(poQuery, cancellationToken);
        
        if (po == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: Authorization check - Users can only cancel their own purchase orders
        var b2bUserQuery = new GetB2BUserByUserIdQuery(userId);
        var b2bUser = await _mediator.Send(b2bUserQuery, cancellationToken);
        
        if (b2bUser == null || po.B2BUserId != b2bUser.Id)
        {
            return Forbid();
        }

        var command = new CancelPurchaseOrderCommand(id);
        var success = await _mediator.Send(command, cancellationToken);
        
        if (!success)
        {
            return BadRequest("Sipariş iptal edilemedi.");
        }
        return NoContent();
    }

    // Volume Discounts
    /// <summary>
    /// Yeni hacim indirimi oluşturur
    /// </summary>
    [HttpPost("volume-discounts")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20/dakika
    [ProducesResponseType(typeof(VolumeDiscountDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<VolumeDiscountDto>> CreateVolumeDiscount([FromBody] CreateVolumeDiscountDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz

        var command = new CreateVolumeDiscountCommand(dto);
        var discount = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetVolumeDiscounts), new { productId = discount.ProductId }, discount);
    }

    /// <summary>
    /// Hacim indirimlerini listeler
    /// </summary>
    [HttpGet("volume-discounts")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<VolumeDiscountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<VolumeDiscountDto>>> GetVolumeDiscounts(
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? organizationId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - Config'den al
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        var query = new GetVolumeDiscountsQuery(productId, categoryId, organizationId, page, pageSize);
        var discounts = await _mediator.Send(query, cancellationToken);
        return Ok(discounts);
    }
}

