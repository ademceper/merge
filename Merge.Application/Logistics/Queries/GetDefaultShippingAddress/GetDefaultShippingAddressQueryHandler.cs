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

namespace Merge.Application.Logistics.Queries.GetDefaultShippingAddress;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetDefaultShippingAddressQueryHandler : IRequestHandler<GetDefaultShippingAddressQuery, ShippingAddressDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetDefaultShippingAddressQueryHandler> _logger;

    public GetDefaultShippingAddressQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetDefaultShippingAddressQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ShippingAddressDto?> Handle(GetDefaultShippingAddressQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting default shipping address. UserId: {UserId}", request.UserId);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        var address = await _context.Set<ShippingAddress>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == request.UserId && a.IsDefault && a.IsActive, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return address != null ? _mapper.Map<ShippingAddressDto>(address) : null;
    }
}

