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

namespace Merge.Application.Logistics.Queries.GetDeliveryTimeEstimationById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class GetDeliveryTimeEstimationByIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetDeliveryTimeEstimationByIdQueryHandler> logger) : IRequestHandler<GetDeliveryTimeEstimationByIdQuery, DeliveryTimeEstimationDto?>
{

    public async Task<DeliveryTimeEstimationDto?> Handle(GetDeliveryTimeEstimationByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting delivery time estimation. EstimationId: {EstimationId}", request.Id);

        var estimation = await context.Set<DeliveryTimeEstimation>()
            .AsNoTracking()
            .Include(e => e.Product)
            .Include(e => e.Category)
            .Include(e => e.Warehouse)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return estimation != null ? mapper.Map<DeliveryTimeEstimationDto>(estimation) : null;
    }
}

