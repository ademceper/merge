using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Seller;
using Merge.Domain.Enums;

namespace Merge.Application.Seller.Queries.GetSellerTransactions;

public record GetSellerTransactionsQuery(
    Guid SellerId,
    SellerTransactionType? TransactionType = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<SellerTransactionDto>>;
