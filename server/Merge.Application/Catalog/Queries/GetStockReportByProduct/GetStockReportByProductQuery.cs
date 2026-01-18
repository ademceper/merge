using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Queries.GetStockReportByProduct;

public record GetStockReportByProductQuery(
    Guid ProductId,
    Guid? PerformedBy = null
) : IRequest<StockReportDto?>;

