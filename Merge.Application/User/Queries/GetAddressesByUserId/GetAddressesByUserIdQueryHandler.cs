using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.User;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.User.Queries.GetAddressesByUserId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetAddressesByUserIdQueryHandler : IRequestHandler<GetAddressesByUserIdQuery, IEnumerable<AddressDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAddressesByUserIdQueryHandler> _logger;

    public GetAddressesByUserIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetAddressesByUserIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<AddressDto>> Handle(GetAddressesByUserIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving addresses for user ID: {UserId}", request.UserId);

        var addresses = await _context.Set<Address>()
            .AsNoTracking()
            .Where(a => a.UserId == request.UserId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} addresses for user ID: {UserId}", addresses.Count, request.UserId);

        return _mapper.Map<IEnumerable<AddressDto>>(addresses);
    }
}
