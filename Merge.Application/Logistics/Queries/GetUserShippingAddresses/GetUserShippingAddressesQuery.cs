using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetUserShippingAddresses;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetUserShippingAddressesQuery(
    Guid UserId,
    bool? IsActive = null) : IRequest<IEnumerable<ShippingAddressDto>>;

