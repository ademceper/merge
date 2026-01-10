using MediatR;
using Merge.Application.DTOs.Logistics;
using Merge.Domain.Enums;

namespace Merge.Application.Logistics.Commands.CreateStockMovement;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
public record CreateStockMovementCommand(
    Guid ProductId,
    Guid WarehouseId,
    StockMovementType MovementType,
    int Quantity,
    string? ReferenceNumber,
    Guid? ReferenceId,
    string? Notes,
    Guid? FromWarehouseId,
    Guid? ToWarehouseId,
    Guid PerformedBy) : IRequest<StockMovementDto>;

