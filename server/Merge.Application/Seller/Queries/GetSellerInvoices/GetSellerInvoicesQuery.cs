using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Seller;
using Merge.Domain.Enums;

namespace Merge.Application.Seller.Queries.GetSellerInvoices;

public record GetSellerInvoicesQuery(
    Guid SellerId,
    SellerInvoiceStatus? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<SellerInvoiceDto>>;
