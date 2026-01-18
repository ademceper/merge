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

namespace Merge.Application.User.Queries.GetAddressById;

public class GetAddressByIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetAddressByIdQueryHandler> logger) : IRequestHandler<GetAddressByIdQuery, AddressDto?>
{

    public async Task<AddressDto?> Handle(GetAddressByIdQuery request, CancellationToken cancellationToken)
    {

        logger.LogInformation("Retrieving address with ID: {AddressId}", request.Id);

        var address =         // ✅ PERFORMANCE: AsNoTracking
        await context.Set<Address>()
            .AsNoTracking()
            .Where(a => a.Id == request.Id && !a.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (address == null)
        {
            logger.LogWarning("Address not found with ID: {AddressId}", request.Id);
            return null;
        }
        if (request.UserId.HasValue && address.UserId != request.UserId.Value && !request.IsAdminOrManager)
        {
            logger.LogWarning("Unauthorized access attempt to address {AddressId} by user {UserId}", 
                request.Id, request.UserId.Value);
            throw new Application.Exceptions.BusinessException("Bu adrese erişim yetkiniz bulunmamaktadır.");
        }

        return mapper.Map<AddressDto>(address);
    }
}
