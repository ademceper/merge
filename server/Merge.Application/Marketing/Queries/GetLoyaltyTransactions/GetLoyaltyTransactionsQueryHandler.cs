using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetLoyaltyTransactions;

public class GetLoyaltyTransactionsQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetLoyaltyTransactionsQuery, PagedResult<LoyaltyTransactionDto>>
{
    public async Task<PagedResult<LoyaltyTransactionDto>> Handle(GetLoyaltyTransactionsQuery request, CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.AddDays(-request.Days);

        var query = context.Set<LoyaltyTransaction>()
            .AsNoTracking()
            .Where(t => t.UserId == request.UserId && t.CreatedAt >= startDate)
            .OrderByDescending(t => t.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var transactions = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<LoyaltyTransactionDto>
        {
            Items = mapper.Map<List<LoyaltyTransactionDto>>(transactions),
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
