using MediatR;

namespace Merge.Application.Logistics.Commands.DeleteDeliveryTimeEstimation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteDeliveryTimeEstimationCommand(Guid Id) : IRequest<Unit>;

