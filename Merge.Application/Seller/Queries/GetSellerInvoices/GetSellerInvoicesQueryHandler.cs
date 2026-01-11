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

namespace Merge.Application.Seller.Queries.GetSellerInvoices;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetSellerInvoicesQueryHandler : IRequestHandler<GetSellerInvoicesQuery, PagedResult<SellerInvoiceDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSellerInvoicesQueryHandler> _logger;
    private readonly PaginationSettings _paginationSettings;

    public GetSellerInvoicesQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetSellerInvoicesQueryHandler> logger,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<SellerInvoiceDto>> Handle(GetSellerInvoicesQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
        _logger.LogInformation("Getting seller invoices. SellerId: {SellerId}, Status: {Status}, Page: {Page}, PageSize: {PageSize}",
            request.SellerId, request.Status?.ToString() ?? "All", request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic number config'den
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize 
            ? _paginationSettings.MaxPageSize 
            : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !i.IsDeleted (Global Query Filter)
        IQueryable<SellerInvoice> query = _context.Set<SellerInvoice>()
            .AsNoTracking()
            .Include(i => i.Seller)
            .Where(i => i.SellerId == request.SellerId);

        // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
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

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var invoiceDtos = _mapper.Map<IEnumerable<SellerInvoiceDto>>(invoices).ToList();

        return new PagedResult<SellerInvoiceDto>
        {
            Items = invoiceDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
