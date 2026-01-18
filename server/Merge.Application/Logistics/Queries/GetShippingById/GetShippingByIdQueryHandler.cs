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

namespace Merge.Application.Logistics.Queries.GetShippingById;

public class GetShippingByIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetShippingByIdQueryHandler> logger) : IRequestHandler<GetShippingByIdQuery, ShippingDto?>
{

    public async Task<ShippingDto?> Handle(GetShippingByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting shipping. ShippingId: {ShippingId}", request.Id);

        var shipping = await context.Set<Shipping>()
            .AsNoTracking()
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        return shipping != null ? mapper.Map<ShippingDto>(shipping) : null;
    }
}

