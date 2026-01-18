using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetInvoice;

public record GetInvoiceQuery(
    Guid InvoiceId
) : IRequest<SellerInvoiceDto?>;
