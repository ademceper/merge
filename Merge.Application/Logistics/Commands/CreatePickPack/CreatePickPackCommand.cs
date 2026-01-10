using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Commands.CreatePickPack;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreatePickPackCommand(
    Guid OrderId,
    Guid WarehouseId,
    string? Notes) : IRequest<PickPackDto>;

