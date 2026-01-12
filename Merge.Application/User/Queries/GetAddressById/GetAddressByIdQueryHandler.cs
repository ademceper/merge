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

namespace Merge.Application.User.Queries.GetAddressById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetAddressByIdQueryHandler : IRequestHandler<GetAddressByIdQuery, AddressDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAddressByIdQueryHandler> _logger;

    public GetAddressByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetAddressByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AddressDto?> Handle(GetAddressByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving address with ID: {AddressId}", request.Id);

        var address = await _context.Set<Address>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (address == null)
        {
            _logger.LogWarning("Address not found with ID: {AddressId}", request.Id);
            return null;
        }

        // ✅ BOLUM 3.2: IDOR koruması - Kullanıcı sadece kendi adreslerine erişebilmeli
        if (request.UserId.HasValue && address.UserId != request.UserId.Value && !request.IsAdminOrManager)
        {
            _logger.LogWarning("Unauthorized access attempt to address {AddressId} by user {UserId}", 
                request.Id, request.UserId.Value);
            throw new Application.Exceptions.BusinessException("Bu adrese erişim yetkiniz bulunmamaktadır.");
        }

        return _mapper.Map<AddressDto>(address);
    }
}
