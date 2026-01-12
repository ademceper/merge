using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Payment;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Payment.Queries.GetInvoiceById;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetInvoiceByIdQueryHandler> _logger;

    public GetInvoiceByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetInvoiceByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<InvoiceDto?> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving invoice. InvoiceId: {InvoiceId}", request.InvoiceId);

        // ✅ PERFORMANCE: AsNoTracking for read-only query
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var invoice = await _context.Set<Invoice>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(i => i.Order)
                .ThenInclude(o => o.Address)
            .Include(i => i.Order)
                .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
            .Include(i => i.Order)
                .ThenInclude(o => o.User)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        if (invoice == null)
        {
            _logger.LogWarning("Invoice not found. InvoiceId: {InvoiceId}", request.InvoiceId);
            return null;
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<InvoiceDto>(invoice);
    }
}
