using MediatR;

namespace Merge.Application.Logistics.Commands.DeleteWarehouse;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteWarehouseCommand(Guid Id) : IRequest<Unit>;

