using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Seller;
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

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/seller/finance")]
[Authorize(Roles = "Seller,Admin")]
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
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetFinanceSummary", new { version, startDate, endDate }, version);
        links["balance"] = new LinkDto { Href = $"/api/v{version}/seller/finance/balance", Method = "GET" };
        links["transactions"] = new LinkDto { Href = $"/api/v{version}/seller/finance/transactions", Method = "GET" };
        links["invoices"] = new LinkDto { Href = $"/api/v{version}/seller/finance/invoices", Method = "GET" };
        return Ok(new { summary, _links = links });
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
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetBalance", new { version }, version);
        links["transactions"] = new LinkDto { Href = $"/api/v{version}/seller/finance/transactions", Method = "GET" };
        links["invoices"] = new LinkDto { Href = $"/api/v{version}/seller/finance/invoices", Method = "GET" };
        links["summary"] = new LinkDto { Href = $"/api/v{version}/seller/finance/summary", Method = "GET" };
        return Ok(new { balance, _links = links });
    }

    [HttpGet("balance/available")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<decimal>> GetAvailableBalance(
        CancellationToken cancellationToken = default)
    {
        var sellerId = GetUserId();
        var query = new GetAvailableBalanceQuery(sellerId);
        var balance = await mediator.Send(query, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetAvailableBalance", new { version }, version);
        return Ok(new { availableBalance = balance, _links = links });
    }

    [HttpGet("balance/pending")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<decimal>> GetPendingBalance(
        CancellationToken cancellationToken = default)
    {
        var sellerId = GetUserId();
        var query = new GetPendingBalanceQuery(sellerId);
        var balance = await mediator.Send(query, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetPendingBalance", new { version }, version);
        return Ok(new { pendingBalance = balance, _links = links });
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
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreatePaginationLinks(Url, "GetTransactions", page, pageSize, result.TotalPages, new { version, transactionType, startDate, endDate }, version);
        return Ok(new { result.Items, result.TotalCount, result.Page, result.PageSize, result.TotalPages, _links = links });
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

        if (transaction == null)
        {
            return NotFound();
        }
        if (transaction.SellerId != sellerId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetTransaction", new { version, id }, version);
        return Ok(new { transaction, _links = links });
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
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreatePaginationLinks(Url, "GetInvoices", page, pageSize, result.TotalPages, new { version, status }, version);
        return Ok(new { result.Items, result.TotalCount, result.Page, result.PageSize, result.TotalPages, _links = links });
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

        if (invoice == null)
        {
            return NotFound();
        }
        if (invoice.SellerId != sellerId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetInvoice", new { version, id }, version);
        if (User.IsInRole("Admin") || User.IsInRole("Manager"))
        {
            if (invoice.Status == SellerInvoiceStatus.Draft)
            {
                links["send"] = new LinkDto { Href = $"/api/v{version}/seller/finance/invoices/{id}/send", Method = "POST" };
            }
            if (invoice.Status == SellerInvoiceStatus.Sent)
            {
                links["markPaid"] = new LinkDto { Href = $"/api/v{version}/seller/finance/invoices/{id}/mark-paid", Method = "POST" };
            }
        }
        return Ok(new { invoice, _links = links });
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
        var links = HateoasHelper.CreateSelfLink(Url, "GetInvoice", new { version, id = invoice.Id }, version);
        links["send"] = new LinkDto { Href = $"/api/v{version}/seller/finance/invoices/{invoice.Id}/send", Method = "POST" };
        links["markPaid"] = new LinkDto { Href = $"/api/v{version}/seller/finance/invoices/{invoice.Id}/mark-paid", Method = "POST" };
        return CreatedAtAction(nameof(GetInvoice), new { version, id = invoice.Id }, new { invoice, _links = links });
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
        {
            return NotFound();
        }
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
        {
            return NotFound();
        }
        return Ok();
    }
}
