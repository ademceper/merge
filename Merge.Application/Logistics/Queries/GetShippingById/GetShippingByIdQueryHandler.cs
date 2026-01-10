using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Logistics.Queries.GetShippingById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetShippingByIdQueryHandler : IRequestHandler<GetShippingByIdQuery, ShippingDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetShippingByIdQueryHandler> _logger;

    public GetShippingByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetShippingByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ShippingDto?> Handle(GetShippingByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting shipping. ShippingId: {ShippingId}", request.Id);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var shipping = await _context.Set<Shipping>()
            .AsNoTracking()
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return shipping != null ? _mapper.Map<ShippingDto>(shipping) : null;
    }
}

