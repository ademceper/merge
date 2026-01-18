using MediatR;

namespace Merge.Application.Logistics.Commands.DeleteWarehouse;

public record DeleteWarehouseCommand(Guid Id) : IRequest<Unit>;

