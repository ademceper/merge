using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Order;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Order.Queries.GetAllReturnRequests;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetAllReturnRequestsQueryHandler : IRequestHandler<GetAllReturnRequestsQuery, PagedResult<ReturnRequestDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly OrderSettings _orderSettings;

    public GetAllReturnRequestsQueryHandler(
        IDbContext context,
        IMapper mapper,
        IOptions<OrderSettings> orderSettings)
    {
        _context = context;
        _mapper = mapper;
        _orderSettings = orderSettings.Value;
    }

    public async Task<PagedResult<ReturnRequestDto>> Handle(GetAllReturnRequestsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - Configuration'dan al
        var pageSize = request.PageSize > _orderSettings.MaxPageSize ? _orderSettings.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        IQueryable<ReturnRequest> query = _context.Set<ReturnRequest>()
            .AsNoTracking()
            .Include(r => r.Order)
            .Include(r => r.User);

        if (!string.IsNullOrEmpty(request.Status))
        {
            var statusEnum = Enum.Parse<ReturnRequestStatus>(request.Status);
            query = query.Where(r => r.Status == statusEnum);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var returnRequests = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Direct Map to List (no intermediate IEnumerable)
        return new PagedResult<ReturnRequestDto>
        {
            Items = _mapper.Map<List<ReturnRequestDto>>(returnRequests),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
