using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Queries.GetStockReportByProduct;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetStockReportByProductQuery(
    Guid ProductId,
    Guid? PerformedBy = null
) : IRequest<StockReportDto?>;

