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
public class GetDeliveryTimeEstimationByIdQueryHandler : IRequestHandler<GetDeliveryTimeEstimationByIdQuery, DeliveryTimeEstimationDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetDeliveryTimeEstimationByIdQueryHandler> _logger;

    public GetDeliveryTimeEstimationByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetDeliveryTimeEstimationByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<DeliveryTimeEstimationDto?> Handle(GetDeliveryTimeEstimationByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting delivery time estimation. EstimationId: {EstimationId}", request.Id);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var estimation = await _context.Set<DeliveryTimeEstimation>()
            .AsNoTracking()
            .Include(e => e.Product)
            .Include(e => e.Category)
            .Include(e => e.Warehouse)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return estimation != null ? _mapper.Map<DeliveryTimeEstimationDto>(estimation) : null;
    }
}

