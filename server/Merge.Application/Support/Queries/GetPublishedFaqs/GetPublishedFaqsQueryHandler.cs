using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Support;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetPublishedFaqs;

public class GetPublishedFaqsQueryHandler(IDbContext context, IMapper mapper, IOptions<SupportSettings> settings) : IRequestHandler<GetPublishedFaqsQuery, PagedResult<FaqDto>>
{
    private readonly SupportSettings supportConfig = settings.Value;

    public async Task<PagedResult<FaqDto>> Handle(GetPublishedFaqsQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize > 0 && request.PageSize <= supportConfig.MaxPageSize 
            ? request.PageSize 
            : supportConfig.DefaultPageSize;
        var page = request.Page > 0 ? request.Page : 1;

        IQueryable<FAQ> query = context.Set<FAQ>()
            .AsNoTracking()
            .Where(f => f.IsPublished);

        if (!string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(f => f.Category == request.Category);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var faqs = await query
            .OrderBy(f => f.SortOrder)
            .ThenBy(f => f.Question)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<FaqDto>
        {
            Items = mapper.Map<List<FaqDto>>(faqs),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
