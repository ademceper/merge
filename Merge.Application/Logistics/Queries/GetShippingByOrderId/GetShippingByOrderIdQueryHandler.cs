using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Queries.GetShippingByOrderId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class GetShippingByOrderIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetShippingByOrderIdQueryHandler> logger) : IRequestHandler<GetShippingByOrderIdQuery, ShippingDto?>
{

    public async Task<ShippingDto?> Handle(GetShippingByOrderIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting shipping by order. OrderId: {OrderId}", request.OrderId);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var shipping = await context.Set<Shipping>()
            .AsNoTracking()
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.OrderId == request.OrderId, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return shipping != null ? mapper.Map<ShippingDto>(shipping) : null;
    }
}

