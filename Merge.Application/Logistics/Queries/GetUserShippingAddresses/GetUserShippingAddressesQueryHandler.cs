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

namespace Merge.Application.Logistics.Queries.GetUserShippingAddresses;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetUserShippingAddressesQueryHandler : IRequestHandler<GetUserShippingAddressesQuery, IEnumerable<ShippingAddressDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserShippingAddressesQueryHandler> _logger;

    public GetUserShippingAddressesQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetUserShippingAddressesQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ShippingAddressDto>> Handle(GetUserShippingAddressesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting user shipping addresses. UserId: {UserId}, IsActive: {IsActive}", request.UserId, request.IsActive);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        var query = _context.Set<ShippingAddress>()
            .AsNoTracking()
            .Where(a => a.UserId == request.UserId);

        if (request.IsActive.HasValue)
        {
            query = query.Where(a => a.IsActive == request.IsActive.Value);
        }

        var addresses = await query
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Label)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (batch mapping)
        return _mapper.Map<IEnumerable<ShippingAddressDto>>(addresses);
    }
}

