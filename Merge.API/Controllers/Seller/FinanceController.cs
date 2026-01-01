using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Seller;
using Merge.Application.DTOs.Seller;

namespace Merge.API.Controllers.Seller;

[ApiController]
[Route("api/seller/finance")]
[Authorize(Roles = "Seller,Admin")]
public class SellerFinanceController : BaseController
{
    private readonly ISellerFinanceService _sellerFinanceService;

    public SellerFinanceController(ISellerFinanceService sellerFinanceService)
    {
        _sellerFinanceService = sellerFinanceService;
    }

    [HttpGet("balance")]
    public async Task<ActionResult<SellerBalanceDto>> GetBalance()
    {
        var sellerId = GetUserId();
        var balance = await _sellerFinanceService.GetSellerBalanceAsync(sellerId);
        return Ok(balance);
    }

    [HttpGet("balance/available")]
    public async Task<ActionResult<decimal>> GetAvailableBalance()
    {
        var sellerId = GetUserId();
        var balance = await _sellerFinanceService.GetAvailableBalanceAsync(sellerId);
        return Ok(new { availableBalance = balance });
    }

    [HttpGet("balance/pending")]
    public async Task<ActionResult<decimal>> GetPendingBalance()
    {
        var sellerId = GetUserId();
        var balance = await _sellerFinanceService.GetPendingBalanceAsync(sellerId);
        return Ok(new { pendingBalance = balance });
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<IEnumerable<SellerTransactionDto>>> GetTransactions(
        [FromQuery] string? transactionType = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var sellerId = GetUserId();
        var transactions = await _sellerFinanceService.GetSellerTransactionsAsync(sellerId, transactionType, startDate, endDate, page, pageSize);
        return Ok(transactions);
    }

    [HttpGet("transactions/{id}")]
    public async Task<ActionResult<SellerTransactionDto>> GetTransaction(Guid id)
    {
        var sellerId = GetUserId();
        var transaction = await _sellerFinanceService.GetTransactionAsync(id);
        if (transaction == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi transaction'larına erişebilir
        if (transaction.SellerId != sellerId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return Ok(transaction);
    }

    [HttpGet("invoices")]
    public async Task<ActionResult<IEnumerable<SellerInvoiceDto>>> GetInvoices(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var sellerId = GetUserId();
        var invoices = await _sellerFinanceService.GetSellerInvoicesAsync(sellerId, status, page, pageSize);
        return Ok(invoices);
    }

    [HttpGet("invoices/{id}")]
    public async Task<ActionResult<SellerInvoiceDto>> GetInvoice(Guid id)
    {
        var sellerId = GetUserId();
        var invoice = await _sellerFinanceService.GetInvoiceAsync(id);
        if (invoice == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi faturalarına erişebilir
        if (invoice.SellerId != sellerId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return Ok(invoice);
    }

    [HttpPost("invoices")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<SellerInvoiceDto>> GenerateInvoice([FromBody] CreateSellerInvoiceDto dto)
    {
        var invoice = await _sellerFinanceService.GenerateInvoiceAsync(dto);
        return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
    }

    [HttpPost("invoices/{id}/mark-paid")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> MarkInvoiceAsPaid(Guid id)
    {
        var success = await _sellerFinanceService.MarkInvoiceAsPaidAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return Ok();
    }

    [HttpGet("summary")]
    public async Task<ActionResult<SellerFinanceSummaryDto>> GetSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var sellerId = GetUserId();
        var summary = await _sellerFinanceService.GetSellerFinanceSummaryAsync(sellerId, startDate, endDate);
        return Ok(summary);
    }
}

