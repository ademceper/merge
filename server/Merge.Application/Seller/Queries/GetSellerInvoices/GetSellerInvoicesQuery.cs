using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Seller;
using Merge.Domain.Enums;

namespace Merge.Application.Seller.Queries.GetSellerInvoices;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
// ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
public record GetSellerInvoicesQuery(
    Guid SellerId,
    SellerInvoiceStatus? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<SellerInvoiceDto>>;
