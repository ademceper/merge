using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetDefaultShippingAddress;

public record GetDefaultShippingAddressQuery(Guid UserId) : IRequest<ShippingAddressDto?>;

