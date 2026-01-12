using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Queries.GetWarehouseByCode;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetWarehouseByCodeQueryHandler : IRequestHandler<GetWarehouseByCodeQuery, WarehouseDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetWarehouseByCodeQueryHandler> _logger;

    public GetWarehouseByCodeQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetWarehouseByCodeQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<WarehouseDto?> Handle(GetWarehouseByCodeQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting warehouse by code. Code: {Code}", request.Code);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        var warehouse = await _context.Set<Warehouse>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Code == request.Code, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return warehouse != null ? _mapper.Map<WarehouseDto>(warehouse) : null;
    }
}

