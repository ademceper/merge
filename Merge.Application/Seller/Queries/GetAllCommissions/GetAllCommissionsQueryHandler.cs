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
using Merge.Domain.Enums;

namespace Merge.Application.Seller.Queries.GetAllCommissions;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetAllCommissionsQueryHandler : IRequestHandler<GetAllCommissionsQuery, PagedResult<SellerCommissionDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllCommissionsQueryHandler> _logger;
    private readonly PaginationSettings _paginationSettings;

    public GetAllCommissionsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetAllCommissionsQueryHandler> logger,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<SellerCommissionDto>> Handle(GetAllCommissionsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting all commissions. Status: {Status}, Page: {Page}, PageSize: {PageSize}",
            request.Status ?? "All", request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic number config'den
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize 
            ? _paginationSettings.MaxPageSize 
            : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sc.IsDeleted (Global Query Filter)
        IQueryable<SellerCommission> query = _context.Set<SellerCommission>()
            .AsNoTracking()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem);

        if (!string.IsNullOrEmpty(request.Status))
        {
            var commissionStatus = Enum.Parse<CommissionStatus>(request.Status, true);
            query = query.Where(sc => sc.Status == commissionStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var commissions = await query
            .OrderByDescending(sc => sc.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var commissionDtos = _mapper.Map<IEnumerable<SellerCommissionDto>>(commissions).ToList();

        return new PagedResult<SellerCommissionDto>
        {
            Items = commissionDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
