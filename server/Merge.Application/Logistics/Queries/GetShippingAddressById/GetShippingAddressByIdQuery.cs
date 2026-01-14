using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetShippingAddressById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetShippingAddressByIdQuery(Guid Id) : IRequest<ShippingAddressDto?>;

