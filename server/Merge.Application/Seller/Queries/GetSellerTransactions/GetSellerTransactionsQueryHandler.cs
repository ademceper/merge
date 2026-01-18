using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetSellerTransactions;

public class GetSellerTransactionsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetSellerTransactionsQueryHandler> logger, IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetSellerTransactionsQuery, PagedResult<SellerTransactionDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;


    public async Task<PagedResult<SellerTransactionDto>> Handle(GetSellerTransactionsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting seller transactions. SellerId: {SellerId}, Type: {TransactionType}, Page: {Page}, PageSize: {PageSize}",
            request.SellerId, request.TransactionType?.ToString() ?? "All", request.Page, request.PageSize);

        var pageSize = request.PageSize > paginationConfig.MaxPageSize 
            ? paginationConfig.MaxPageSize 
            : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        IQueryable<SellerTransaction> query = context.Set<SellerTransaction>()
            .AsNoTracking()
            .Include(t => t.Seller)
            .Where(t => t.SellerId == request.SellerId);

        if (request.TransactionType.HasValue)
        {
            query = query.Where(t => t.TransactionType == request.TransactionType.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= request.EndDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var transactions = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var transactionDtos = mapper.Map<IEnumerable<SellerTransactionDto>>(transactions).ToList();

        return new PagedResult<SellerTransactionDto>
        {
            Items = transactionDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
