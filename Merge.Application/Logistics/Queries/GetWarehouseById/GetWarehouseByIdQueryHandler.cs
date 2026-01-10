using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Logistics.Queries.GetWarehouseById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetWarehouseByIdQueryHandler : IRequestHandler<GetWarehouseByIdQuery, WarehouseDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetWarehouseByIdQueryHandler> _logger;

    public GetWarehouseByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetWarehouseByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<WarehouseDto?> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting warehouse. WarehouseId: {WarehouseId}", request.Id);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        var warehouse = await _context.Set<Warehouse>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return warehouse != null ? _mapper.Map<WarehouseDto>(warehouse) : null;
    }
}

