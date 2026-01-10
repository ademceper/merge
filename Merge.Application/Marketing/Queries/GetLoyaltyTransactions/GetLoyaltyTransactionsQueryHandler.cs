using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Marketing.Queries.GetLoyaltyTransactions;

public class GetLoyaltyTransactionsQueryHandler : IRequestHandler<GetLoyaltyTransactionsQuery, PagedResult<LoyaltyTransactionDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetLoyaltyTransactionsQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<LoyaltyTransactionDto>> Handle(GetLoyaltyTransactionsQuery request, CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.AddDays(-request.Days);

        var query = _context.Set<LoyaltyTransaction>()
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
            Items = _mapper.Map<List<LoyaltyTransactionDto>>(transactions),
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
