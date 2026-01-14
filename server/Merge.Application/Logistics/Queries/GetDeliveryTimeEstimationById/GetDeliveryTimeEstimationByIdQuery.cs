using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetDeliveryTimeEstimationById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetDeliveryTimeEstimationByIdQuery(Guid Id) : IRequest<DeliveryTimeEstimationDto?>;

