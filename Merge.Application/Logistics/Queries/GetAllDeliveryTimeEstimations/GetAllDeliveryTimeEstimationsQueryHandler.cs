using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Logistics.Queries.GetAllDeliveryTimeEstimations;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetAllDeliveryTimeEstimationsQueryHandler : IRequestHandler<GetAllDeliveryTimeEstimationsQuery, IEnumerable<DeliveryTimeEstimationDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllDeliveryTimeEstimationsQueryHandler> _logger;

    public GetAllDeliveryTimeEstimationsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetAllDeliveryTimeEstimationsQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<DeliveryTimeEstimationDto>> Handle(GetAllDeliveryTimeEstimationsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all delivery time estimations. ProductId: {ProductId}, CategoryId: {CategoryId}, WarehouseId: {WarehouseId}, IsActive: {IsActive}",
            request.ProductId, request.CategoryId, request.WarehouseId, request.IsActive);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        IQueryable<DeliveryTimeEstimation> query = _context.Set<DeliveryTimeEstimation>()
            .AsNoTracking()
            .Include(e => e.Product)
            .Include(e => e.Category)
            .Include(e => e.Warehouse);

        if (request.ProductId.HasValue)
        {
            query = query.Where(e => e.ProductId == request.ProductId.Value);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(e => e.CategoryId == request.CategoryId.Value);
        }

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(e => e.WarehouseId == request.WarehouseId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(e => e.IsActive == request.IsActive.Value);
        }

        var estimations = await query
            .OrderBy(e => e.AverageDays)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (batch mapping)
        return _mapper.Map<IEnumerable<DeliveryTimeEstimationDto>>(estimations);
    }
}

