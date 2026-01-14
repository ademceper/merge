using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Payment;
using Merge.Application.Common;
using Merge.Application.Payment.Queries.GetInvoiceById;
using Merge.Application.Payment.Queries.GetInvoiceByOrderId;
using Merge.Application.Payment.Queries.GetInvoicesByUserId;
using Merge.Application.Payment.Commands.GenerateInvoice;
using Merge.Application.Payment.Commands.SendInvoice;
using Merge.Application.Payment.Commands.GenerateInvoicePdf;
using Merge.Application.Order.Queries.GetOrderById;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Payment;

[ApiController]
[Route("api/v{version:apiVersion}/payments/invoices")]
[Authorize]
public class InvoicesController : BaseController
{
    private readonly IMediator _mediator;

    public InvoicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<InvoiceDto>>> GetMyInvoices(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var userId = GetUserId();
        var query = new GetInvoicesByUserIdQuery(userId, page, pageSize);
        var invoices = await _mediator.Send(query, cancellationToken);
        return Ok(invoices);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<InvoiceDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetInvoiceByIdQuery(id);
        var invoice = await _mediator.Send(query, cancellationToken);
        if (invoice == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi siparişlerinin faturalarına erişebilmeli
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var orderQuery = new GetOrderByIdQuery(invoice.OrderId);
        var order = await _mediator.Send(orderQuery, cancellationToken);
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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("order/{orderId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<InvoiceDto>> GetByOrderId(Guid orderId, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Önce Order'ın kullanıcıya ait olduğunu kontrol et
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var orderQuery = new GetOrderByIdQuery(orderId);
        var order = await _mediator.Send(orderQuery, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetInvoiceByOrderIdQuery(orderId);
        var invoice = await _mediator.Send(query, cancellationToken);
        if (invoice == null)
        {
            return NotFound();
        }
        
        return Ok(invoice);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("generate/{orderId}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<InvoiceDto>> GenerateInvoice(Guid orderId, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var command = new GenerateInvoiceCommand(orderId);
        var invoice = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("{id}/send")]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendInvoice(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var command = new SendInvoiceCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("{id}/pdf")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika (PDF generation is expensive)
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<string>> GetInvoicePdf(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetInvoiceByIdQuery(id);
        var invoice = await _mediator.Send(query, cancellationToken);
        if (invoice == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi siparişlerinin faturalarına erişebilmeli
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var orderQuery = new GetOrderByIdQuery(invoice.OrderId);
        var order = await _mediator.Send(orderQuery, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new GenerateInvoicePdfCommand(id);
        var pdfUrl = await _mediator.Send(command, cancellationToken);
        return Ok(new { pdfUrl });
    }
}

