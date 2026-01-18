using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetShippingAddressById;

public record GetShippingAddressByIdQuery(Guid Id) : IRequest<ShippingAddressDto?>;

