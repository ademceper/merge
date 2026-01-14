using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetStockByWarehouse;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetStockByWarehouseQuery() : IRequest<List<WarehouseStockDto>>;

