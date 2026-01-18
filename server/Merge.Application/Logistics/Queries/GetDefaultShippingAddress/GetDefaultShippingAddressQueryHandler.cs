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

namespace Merge.Application.Logistics.Queries.GetDefaultShippingAddress;

public class GetDefaultShippingAddressQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetDefaultShippingAddressQueryHandler> logger) : IRequestHandler<GetDefaultShippingAddressQuery, ShippingAddressDto?>
{

    public async Task<ShippingAddressDto?> Handle(GetDefaultShippingAddressQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting default shipping address. UserId: {UserId}", request.UserId);

        var address = await context.Set<ShippingAddress>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == request.UserId && a.IsDefault && a.IsActive, cancellationToken);

        return address != null ? mapper.Map<ShippingAddressDto>(address) : null;
    }
}

