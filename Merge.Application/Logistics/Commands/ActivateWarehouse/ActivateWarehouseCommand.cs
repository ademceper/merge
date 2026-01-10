using MediatR;

namespace Merge.Application.Logistics.Commands.ActivateWarehouse;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ActivateWarehouseCommand(Guid Id) : IRequest<Unit>;

