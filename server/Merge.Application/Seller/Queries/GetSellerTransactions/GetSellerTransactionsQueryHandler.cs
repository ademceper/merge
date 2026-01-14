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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetSellerTransactionsQueryHandler : IRequestHandler<GetSellerTransactionsQuery, PagedResult<SellerTransactionDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSellerTransactionsQueryHandler> _logger;
    private readonly PaginationSettings _paginationSettings;

    public GetSellerTransactionsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetSellerTransactionsQueryHandler> logger,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<SellerTransactionDto>> Handle(GetSellerTransactionsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting seller transactions. SellerId: {SellerId}, Type: {TransactionType}, Page: {Page}, PageSize: {PageSize}",
            request.SellerId, request.TransactionType?.ToString() ?? "All", request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic number config'den
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize 
            ? _paginationSettings.MaxPageSize 
            : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        IQueryable<SellerTransaction> query = _context.Set<SellerTransaction>()
            .AsNoTracking()
            .Include(t => t.Seller)
            .Where(t => t.SellerId == request.SellerId);

        // ✅ ARCHITECTURE: Enum kullanımı (string TransactionType yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
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

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var transactionDtos = _mapper.Map<IEnumerable<SellerTransactionDto>>(transactions).ToList();

        return new PagedResult<SellerTransactionDto>
        {
            Items = transactionDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
