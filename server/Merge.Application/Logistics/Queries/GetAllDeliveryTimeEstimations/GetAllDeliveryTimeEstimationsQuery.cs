using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetAllDeliveryTimeEstimations;

public record GetAllDeliveryTimeEstimationsQuery(
    Guid? ProductId,
    Guid? CategoryId,
    Guid? WarehouseId,
    bool? IsActive) : IRequest<IEnumerable<DeliveryTimeEstimationDto>>;

