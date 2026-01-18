using MediatR;

namespace Merge.Application.Logistics.Commands.DeactivateWarehouse;

public record DeactivateWarehouseCommand(Guid Id) : IRequest<Unit>;

