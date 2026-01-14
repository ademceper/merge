using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetInvoice;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetInvoiceQuery(
    Guid InvoiceId
) : IRequest<SellerInvoiceDto?>;
