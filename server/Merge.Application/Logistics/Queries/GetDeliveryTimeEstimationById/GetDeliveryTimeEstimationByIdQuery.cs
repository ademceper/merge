using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetDeliveryTimeEstimationById;

public record GetDeliveryTimeEstimationByIdQuery(Guid Id) : IRequest<DeliveryTimeEstimationDto?>;

