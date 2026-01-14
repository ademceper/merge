using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetShippingByOrderId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetShippingByOrderIdQuery(Guid OrderId) : IRequest<ShippingDto?>;

