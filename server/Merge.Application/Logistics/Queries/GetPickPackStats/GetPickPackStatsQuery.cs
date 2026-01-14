using MediatR;

namespace Merge.Application.Logistics.Queries.GetPickPackStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ⚠️ NOTE: Dictionary<string, int> burada kabul edilebilir çünkü stats için key-value çiftleri dinamik
public record GetPickPackStatsQuery(
    Guid? WarehouseId,
    DateTime? StartDate,
    DateTime? EndDate) : IRequest<Dictionary<string, int>>;

