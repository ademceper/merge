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

namespace Merge.Application.Logistics.Queries.GetShippingAddressById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class GetShippingAddressByIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetShippingAddressByIdQueryHandler> logger) : IRequestHandler<GetShippingAddressByIdQuery, ShippingAddressDto?>
{

    public async Task<ShippingAddressDto?> Handle(GetShippingAddressByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting shipping address. AddressId: {AddressId}", request.Id);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        var address = await context.Set<ShippingAddress>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return address != null ? mapper.Map<ShippingAddressDto>(address) : null;
    }
}

