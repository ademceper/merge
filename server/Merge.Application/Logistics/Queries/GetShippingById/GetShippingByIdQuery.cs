using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetShippingById;

public record GetShippingByIdQuery(Guid Id) : IRequest<ShippingDto?>;

