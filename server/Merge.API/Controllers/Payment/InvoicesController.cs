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

/// <summary>
/// Invoices API endpoints.
/// Faturaları yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/payments/invoices")]
[Authorize]
[Tags("Invoices")]
public class InvoicesController(IMediator mediator) : BaseController
{
    [HttpGet]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<InvoiceDto>>> GetMyInvoices(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetInvoicesByUserIdQuery(userId, page, pageSize);
        var invoices = await mediator.Send(query, cancellationToken);
        return Ok(invoices);
    }

    [HttpGet("{id}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<InvoiceDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var query = new GetInvoiceByIdQuery(id);
        var invoice = await mediator.Send(query, cancellationToken);
        if (invoice is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        var orderQuery = new GetOrderByIdQuery(invoice.OrderId);
        var order = await mediator.Send(orderQuery, cancellationToken);
        if (order is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        return Ok(invoice);
    }

    [HttpGet("order/{orderId}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<InvoiceDto>> GetByOrderId(Guid orderId, CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var orderQuery = new GetOrderByIdQuery(orderId);
        var order = await mediator.Send(orderQuery, cancellationToken);
        if (order is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        var query = new GetInvoiceByOrderIdQuery(orderId);
        var invoice = await mediator.Send(query, cancellationToken);
        if (invoice is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok(invoice);
    }

    [HttpPost("generate/{orderId}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<InvoiceDto>> GenerateInvoice(Guid orderId, CancellationToken cancellationToken = default)
    {
        var command = new GenerateInvoiceCommand(orderId);
        var invoice = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice);
    }

    [HttpPost("{id}/send")]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendInvoice(Guid id, CancellationToken cancellationToken = default)
    {
        var command = new SendInvoiceCommand(id);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    /// <summary>
    /// Faturanın PDF dosyasını getirir
    /// </summary>
    /// <param name="id">Fatura ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>PDF dosyası URL'i</returns>
    /// <response code="200">PDF URL'i başarıyla getirildi</response>
    /// <response code="401">Kimlik doğrulama gerekli</response>
    /// <response code="403">Yetki yok</response>
    /// <response code="404">Fatura bulunamadı</response>
    /// <response code="429">Rate limit aşıldı</response>
    [HttpGet("{id}/pdf")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<string>> GetInvoicePdf(Guid id, CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var query = new GetInvoiceByIdQuery(id);
        var invoice = await mediator.Send(query, cancellationToken);
        if (invoice is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        var orderQuery = new GetOrderByIdQuery(invoice.OrderId);
        var order = await mediator.Send(orderQuery, cancellationToken);
        if (order is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        var command = new GenerateInvoicePdfCommand(id);
        var pdfUrl = await mediator.Send(command, cancellationToken);
        return Ok(pdfUrl);
    }
}
