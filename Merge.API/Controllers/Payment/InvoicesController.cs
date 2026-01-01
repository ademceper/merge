using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Payment;
using Merge.Application.Interfaces.Order;
using Merge.Application.DTOs.Payment;


namespace Merge.API.Controllers.Payment;

[ApiController]
[Route("api/payments/invoices")]
[Authorize]
public class InvoicesController : BaseController
{
    private readonly IInvoiceService _invoiceService;
    private readonly IOrderService _orderService;

    public InvoicesController(IInvoiceService invoiceService, IOrderService orderService)
    {
        _invoiceService = invoiceService;
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetMyInvoices()
    {
        var userId = GetUserId();
        var invoices = await _invoiceService.GetByUserIdAsync(userId);
        return Ok(invoices);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InvoiceDto>> GetById(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var invoice = await _invoiceService.GetByIdAsync(id);
        if (invoice == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi siparişlerinin faturalarına erişebilmeli
        var order = await _orderService.GetByIdAsync(invoice.OrderId);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        
        return Ok(invoice);
    }

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<InvoiceDto>> GetByOrderId(Guid orderId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Önce Order'ın kullanıcıya ait olduğunu kontrol et
        var order = await _orderService.GetByIdAsync(orderId);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var invoice = await _invoiceService.GetByOrderIdAsync(orderId);
        if (invoice == null)
        {
            return NotFound();
        }
        
        return Ok(invoice);
    }

    [HttpPost("generate/{orderId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<InvoiceDto>> GenerateInvoice(Guid orderId)
    {
        var invoice = await _invoiceService.GenerateInvoiceAsync(orderId);
        if (invoice == null)
        {
            return NotFound();
        }
        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice);
    }

    [HttpPost("{id}/send")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendInvoice(Guid id)
    {
        var result = await _invoiceService.SendInvoiceAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpGet("{id}/pdf")]
    public async Task<ActionResult<string>> GetInvoicePdf(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var invoice = await _invoiceService.GetByIdAsync(id);
        if (invoice == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi siparişlerinin faturalarına erişebilmeli
        var order = await _orderService.GetByIdAsync(invoice.OrderId);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var pdfUrl = await _invoiceService.GenerateInvoicePdfAsync(id);
        return Ok(new { pdfUrl });
    }
}

