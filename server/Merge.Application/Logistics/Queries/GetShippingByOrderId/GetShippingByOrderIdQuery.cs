using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetShippingByOrderId;

public record GetShippingByOrderIdQuery(Guid OrderId) : IRequest<ShippingDto?>;

