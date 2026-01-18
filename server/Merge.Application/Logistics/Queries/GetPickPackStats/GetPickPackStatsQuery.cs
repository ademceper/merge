using MediatR;

namespace Merge.Application.Logistics.Queries.GetPickPackStats;

// ⚠️ NOTE: Dictionary<string, int> burada kabul edilebilir çünkü stats için key-value çiftleri dinamik
public record GetPickPackStatsQuery(
    Guid? WarehouseId,
    DateTime? StartDate,
    DateTime? EndDate) : IRequest<Dictionary<string, int>>;

