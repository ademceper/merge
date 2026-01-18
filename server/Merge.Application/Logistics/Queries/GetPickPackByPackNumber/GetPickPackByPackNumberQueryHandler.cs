using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Queries.GetPickPackByPackNumber;

public class GetPickPackByPackNumberQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetPickPackByPackNumberQueryHandler> logger) : IRequestHandler<GetPickPackByPackNumberQuery, PickPackDto?>
{

    public async Task<PickPackDto?> Handle(GetPickPackByPackNumberQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting pick-pack by pack number. PackNumber: {PackNumber}", request.PackNumber);

        var pickPack = await context.Set<PickPack>()
            .AsNoTracking()
            .Include(pp => pp.Order)
            .Include(pp => pp.Warehouse)
            .Include(pp => pp.PickedBy)
            .Include(pp => pp.PackedBy)
            .Include(pp => pp.Items)
                .ThenInclude(i => i.OrderItem)
                    .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(pp => pp.PackNumber == request.PackNumber, cancellationToken);

        return pickPack is not null ? mapper.Map<PickPackDto>(pickPack) : null;
    }
}

