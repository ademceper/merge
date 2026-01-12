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

namespace Merge.Application.Logistics.Queries.GetPickPacksByOrderId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetPickPacksByOrderIdQueryHandler : IRequestHandler<GetPickPacksByOrderIdQuery, IEnumerable<PickPackDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPickPacksByOrderIdQueryHandler> _logger;

    public GetPickPacksByOrderIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetPickPacksByOrderIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<PickPackDto>> Handle(GetPickPacksByOrderIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting pick-packs by order. OrderId: {OrderId}", request.OrderId);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var pickPacks = await _context.Set<PickPack>()
            .AsNoTracking()
            .Include(pp => pp.Order)
            .Include(pp => pp.Warehouse)
            .Include(pp => pp.PickedBy)
            .Include(pp => pp.PackedBy)
            .Include(pp => pp.Items)
                .ThenInclude(i => i.OrderItem)
                    .ThenInclude(oi => oi.Product)
            .Where(pp => pp.OrderId == request.OrderId)
            .OrderByDescending(pp => pp.CreatedAt)
            .Take(50) // ✅ Güvenlik: Maksimum 50 pick-pack
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (batch mapping)
        return _mapper.Map<IEnumerable<PickPackDto>>(pickPacks);
    }
}

