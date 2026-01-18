using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetSellerInvoices;

public class GetSellerInvoicesQueryHandler(IDbContext context, IMapper mapper, ILogger<GetSellerInvoicesQueryHandler> logger, IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetSellerInvoicesQuery, PagedResult<SellerInvoiceDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;


    public async Task<PagedResult<SellerInvoiceDto>> Handle(GetSellerInvoicesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting seller invoices. SellerId: {SellerId}, Status: {Status}, Page: {Page}, PageSize: {PageSize}",
            request.SellerId, request.Status?.ToString() ?? "All", request.Page, request.PageSize);

        var pageSize = request.PageSize > paginationConfig.MaxPageSize 
            ? paginationConfig.MaxPageSize 
            : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        IQueryable<SellerInvoice> query = context.Set<SellerInvoice>()
            .AsNoTracking()
            .Include(i => i.Seller)
            .Where(i => i.SellerId == request.SellerId);

        if (request.Status.HasValue)
        {
            query = query.Where(i => i.Status == request.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var invoices = await query
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var invoiceDtos = mapper.Map<IEnumerable<SellerInvoiceDto>>(invoices).ToList();

        return new PagedResult<SellerInvoiceDto>
        {
            Items = invoiceDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
