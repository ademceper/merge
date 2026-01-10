using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetStockMovementsByInventoryId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
public record GetStockMovementsByInventoryIdQuery(Guid InventoryId) : IRequest<IEnumerable<StockMovementDto>>;

