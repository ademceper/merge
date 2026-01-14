using MediatR;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;

namespace Merge.Application.Marketing.Queries.GetLoyaltyTransactions;

public record GetLoyaltyTransactionsQuery(
    Guid UserId,
    int Days = 30,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<LoyaltyTransactionDto>>;
