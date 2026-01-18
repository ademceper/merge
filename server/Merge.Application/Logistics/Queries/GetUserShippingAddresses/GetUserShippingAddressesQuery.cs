using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetUserShippingAddresses;

public record GetUserShippingAddressesQuery(
    Guid UserId,
    bool? IsActive = null) : IRequest<IEnumerable<ShippingAddressDto>>;

