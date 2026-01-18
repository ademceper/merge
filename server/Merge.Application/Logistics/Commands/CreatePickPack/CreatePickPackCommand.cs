using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Commands.CreatePickPack;

public record CreatePickPackCommand(
    Guid OrderId,
    Guid WarehouseId,
    string? Notes) : IRequest<PickPackDto>;

