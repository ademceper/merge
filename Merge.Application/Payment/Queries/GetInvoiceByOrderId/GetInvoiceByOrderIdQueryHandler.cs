using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Payment;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Payment.Queries.GetInvoiceByOrderId;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class GetInvoiceByOrderIdQueryHandler : IRequestHandler<GetInvoiceByOrderIdQuery, InvoiceDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetInvoiceByOrderIdQueryHandler> _logger;

    public GetInvoiceByOrderIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetInvoiceByOrderIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<InvoiceDto?> Handle(GetInvoiceByOrderIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving invoice by order ID. OrderId: {OrderId}", request.OrderId);

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
            .FirstOrDefaultAsync(i => i.OrderId == request.OrderId, cancellationToken);

        if (invoice == null)
        {
            _logger.LogWarning("Invoice not found for order ID. OrderId: {OrderId}", request.OrderId);
            return null;
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<InvoiceDto>(invoice);
    }
}
