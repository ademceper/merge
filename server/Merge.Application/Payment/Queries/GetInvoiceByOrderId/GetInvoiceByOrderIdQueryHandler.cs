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

namespace Merge.Application.Payment.Queries.GetInvoiceByOrderId;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class GetInvoiceByOrderIdQueryHandler(IDbContext context, IMapper mapper, ILogger<GetInvoiceByOrderIdQueryHandler> logger) : IRequestHandler<GetInvoiceByOrderIdQuery, InvoiceDto?>
{

    public async Task<InvoiceDto?> Handle(GetInvoiceByOrderIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving invoice by order ID. OrderId: {OrderId}", request.OrderId);

        var invoice = await context.Set<Invoice>()
            .AsNoTracking()
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
            logger.LogWarning("Invoice not found for order ID. OrderId: {OrderId}", request.OrderId);
            return null;
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<InvoiceDto>(invoice);
    }
}
