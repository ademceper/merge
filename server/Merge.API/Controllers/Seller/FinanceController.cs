using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Seller;
using Merge.Application.Exceptions;
using Merge.API.Middleware;
using Merge.API.Helpers;
using Merge.Application.Common;
using Merge.Application.Seller.Queries.GetSellerBalance;
using Merge.Application.Seller.Queries.GetAvailableBalance;
using Merge.Application.Seller.Queries.GetPendingBalance;
using Merge.Application.Seller.Queries.GetSellerTransactions;
using Merge.Application.Seller.Queries.GetTransaction;
using Merge.Application.Seller.Queries.GetSellerInvoices;
using Merge.Application.Seller.Queries.GetInvoice;
using Merge.Application.Seller.Queries.GetSellerFinanceSummary;
using Merge.Application.Seller.Commands.GenerateInvoice;
using Merge.Application.Seller.Commands.MarkInvoiceAsPaid;
using Merge.Application.Seller.Commands.SendInvoice;
using Merge.Domain.Enums;

namespace Merge.API.Controllers.Seller;

/// <summary>
/// Seller Finance API endpoints.
/// Satıcı finansal işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/seller/finance")]
[Authorize(Roles = "Seller,Admin")]
[Tags("SellerFinance")]
public class FinanceController(IMediator mediator) : BaseController
{

    [HttpGet("summary")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(SellerFinanceSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SellerFinanceSummaryDto>> GetFinanceSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var sellerId = GetUserId();
        var query = new GetSellerFinanceSummaryQuery(sellerId, startDate, endDate);
        var summary = await mediator.Send(query, cancellationToken);
        return Ok(summary);
    }

    [HttpGet("balance")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(SellerBalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SellerBalanceDto>> GetBalance(
        CancellationToken cancellationToken = default)
    {
        var sellerId = GetUserId();
        var query = new GetSellerBalanceQuery(sellerId);
        var balance = await mediator.Send(query, cancellationToken);
        return Ok(balance);
    }

    /// <summary>
    /// Kullanılabilir bakiye tutarını getirir
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kullanılabilir bakiye tutarı</returns>
    /// <response code="200">Tutar başarıyla getirildi</response>
    /// <response code="401">Kimlik doğrulama gerekli</response>
    /// <response code="403">Yetki yok</response>
    /// <response code="429">Rate limit aşıldı</response>
    [HttpGet("balance/available")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<decimal>> GetAvailableBalance(
        CancellationToken cancellationToken = default)
    {
        var sellerId = GetUserId();
        var query = new GetAvailableBalanceQuery(sellerId);
        var balance = await mediator.Send(query, cancellationToken);
        return Ok(balance);
    }

    /// <summary>
    /// Bekleyen bakiye tutarını getirir
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Bekleyen bakiye tutarı</returns>
    /// <response code="200">Tutar başarıyla getirildi</response>
    /// <response code="401">Kimlik doğrulama gerekli</response>
    /// <response code="403">Yetki yok</response>
    /// <response code="429">Rate limit aşıldı</response>
    [HttpGet("balance/pending")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<decimal>> GetPendingBalance(
        CancellationToken cancellationToken = default)
    {
        var sellerId = GetUserId();
        var query = new GetPendingBalanceQuery(sellerId);
        var balance = await mediator.Send(query, cancellationToken);
        return Ok(balance);
    }

    [HttpGet("transactions")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<SellerTransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<SellerTransactionDto>>> GetTransactions(
        [FromQuery] SellerTransactionType? transactionType = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var sellerId = GetUserId();
        var query = new GetSellerTransactionsQuery(sellerId, transactionType, startDate, endDate, page, pageSize);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("transactions/{id}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(SellerTransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SellerTransactionDto>> GetTransaction(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var sellerId = GetUserId();
        var query = new GetTransactionQuery(id);
        var transaction = await mediator.Send(query, cancellationToken);

        if (transaction is null)
            throw new NotFoundException("SellerTransaction", id);

        if (transaction.SellerId != sellerId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }
        return Ok(transaction);
    }

    [HttpGet("invoices")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<SellerInvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<SellerInvoiceDto>>> GetInvoices(
        [FromQuery] SellerInvoiceStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var sellerId = GetUserId();
        var query = new GetSellerInvoicesQuery(sellerId, status, page, pageSize);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("invoices/{id}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(SellerInvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SellerInvoiceDto>> GetInvoice(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var sellerId = GetUserId();
        var query = new GetInvoiceQuery(id);
        var invoice = await mediator.Send(query, cancellationToken);

        if (invoice is null)
            throw new NotFoundException("SellerInvoice", id);

        if (invoice.SellerId != sellerId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }
        return Ok(invoice);
    }

    [HttpPost("invoices")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(SellerInvoiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SellerInvoiceDto>> GenerateInvoice(
        [FromBody] CreateSellerInvoiceDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new GenerateInvoiceCommand(dto);
        var invoice = await mediator.Send(command, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetInvoice), new { version, id = invoice.Id }, invoice);
    }

    [HttpPost("invoices/{id}/send")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendInvoice(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new SendInvoiceCommand(id);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
            throw new NotFoundException("SellerInvoice", id);

        return NoContent();
    }

    [HttpPost("invoices/{id}/mark-paid")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> MarkInvoiceAsPaid(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new MarkInvoiceAsPaidCommand(id);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
            throw new NotFoundException("SellerInvoice", id);

        return Ok();
    }
}
