using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetStockByWarehouse;

public record GetStockByWarehouseQuery() : IRequest<List<WarehouseStockDto>>;

