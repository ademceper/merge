using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetSellerFinanceSummary;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetSellerFinanceSummaryQuery(
    Guid SellerId,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<SellerFinanceSummaryDto>;
