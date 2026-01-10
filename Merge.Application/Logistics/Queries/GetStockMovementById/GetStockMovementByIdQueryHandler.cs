using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Logistics.Queries.GetStockMovementById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetStockMovementByIdQueryHandler : IRequestHandler<GetStockMovementByIdQuery, StockMovementDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetStockMovementByIdQueryHandler> _logger;

    public GetStockMovementByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetStockMovementByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<StockMovementDto?> Handle(GetStockMovementByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting stock movement. StockMovementId: {StockMovementId}", request.Id);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var movement = await _context.Set<StockMovement>()
            .AsNoTracking()
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Include(sm => sm.User)
            .Include(sm => sm.FromWarehouse)
            .Include(sm => sm.ToWarehouse)
            .FirstOrDefaultAsync(sm => sm.Id == request.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return movement != null ? _mapper.Map<StockMovementDto>(movement) : null;
    }
}

