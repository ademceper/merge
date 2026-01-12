using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Payment;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Payment.Queries.GetInvoicesByUserId;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
// BOLUM 3.4: Pagination (ZORUNLU)
public class GetInvoicesByUserIdQueryHandler : IRequestHandler<GetInvoicesByUserIdQuery, PagedResult<InvoiceDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetInvoicesByUserIdQueryHandler> _logger;

    public GetInvoicesByUserIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetInvoicesByUserIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<InvoiceDto>> Handle(GetInvoicesByUserIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving invoices by user ID. UserId: {UserId}, Page: {Page}, PageSize: {PageSize}",
            request.UserId, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > 100 ? 100 : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking for read-only query
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var query = _context.Set<Invoice>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(i => i.Order)
                .ThenInclude(o => o.Address)
            .Include(i => i.Order)
                .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
            .Include(i => i.Order)
                .ThenInclude(o => o.User)
            .Where(i => i.Order.UserId == request.UserId);

        var totalCount = await query.CountAsync(cancellationToken);

        var invoices = await query
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper kullan
        var dtos = _mapper.Map<IEnumerable<InvoiceDto>>(invoices);

        return new PagedResult<InvoiceDto>
        {
            Items = dtos.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
