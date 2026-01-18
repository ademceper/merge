using MediatR;

namespace Merge.Application.Logistics.Commands.DeleteDeliveryTimeEstimation;

public record DeleteDeliveryTimeEstimationCommand(Guid Id) : IRequest<Unit>;

