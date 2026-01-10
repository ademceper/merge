using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetDefaultShippingAddress;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetDefaultShippingAddressQuery(Guid UserId) : IRequest<ShippingAddressDto?>;

