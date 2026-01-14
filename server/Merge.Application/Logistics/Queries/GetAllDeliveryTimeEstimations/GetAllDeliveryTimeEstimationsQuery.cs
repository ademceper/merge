using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetAllDeliveryTimeEstimations;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAllDeliveryTimeEstimationsQuery(
    Guid? ProductId,
    Guid? CategoryId,
    Guid? WarehouseId,
    bool? IsActive) : IRequest<IEnumerable<DeliveryTimeEstimationDto>>;

