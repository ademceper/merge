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

namespace Merge.Application.Logistics.Queries.GetShippingAddressById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetShippingAddressByIdQueryHandler : IRequestHandler<GetShippingAddressByIdQuery, ShippingAddressDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetShippingAddressByIdQueryHandler> _logger;

    public GetShippingAddressByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetShippingAddressByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ShippingAddressDto?> Handle(GetShippingAddressByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting shipping address. AddressId: {AddressId}", request.Id);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        var address = await _context.Set<ShippingAddress>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return address != null ? _mapper.Map<ShippingAddressDto>(address) : null;
    }
}

