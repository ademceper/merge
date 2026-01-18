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
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Order.Queries.GetAllReturnRequests;

public class GetAllReturnRequestsQueryHandler(IDbContext context, IMapper mapper, IOptions<OrderSettings> orderSettings) : IRequestHandler<GetAllReturnRequestsQuery, PagedResult<ReturnRequestDto>>
{
    private readonly OrderSettings orderConfig = orderSettings.Value;

    public async Task<PagedResult<ReturnRequestDto>> Handle(GetAllReturnRequestsQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize > orderConfig.MaxPageSize ? orderConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        IQueryable<ReturnRequest> query = context.Set<ReturnRequest>()
            .AsNoTracking()
            .AsSplitQuery()
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

        return new PagedResult<ReturnRequestDto>
        {
            Items = mapper.Map<List<ReturnRequestDto>>(returnRequests),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
