using MediatR;

namespace Merge.Application.Logistics.Commands.DeactivateWarehouse;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeactivateWarehouseCommand(Guid Id) : IRequest<Unit>;

