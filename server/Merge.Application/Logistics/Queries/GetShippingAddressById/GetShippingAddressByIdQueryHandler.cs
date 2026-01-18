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

public class GetShippingAddressByIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetShippingAddressByIdQueryHandler> logger) : IRequestHandler<GetShippingAddressByIdQuery, ShippingAddressDto?>
{

    public async Task<ShippingAddressDto?> Handle(GetShippingAddressByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting shipping address. AddressId: {AddressId}", request.Id);

        var address = await context.Set<ShippingAddress>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        return address != null ? mapper.Map<ShippingAddressDto>(address) : null;
    }
}

