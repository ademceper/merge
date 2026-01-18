using MediatR;

namespace Merge.Application.Logistics.Commands.ActivateWarehouse;

public record ActivateWarehouseCommand(Guid Id) : IRequest<Unit>;

