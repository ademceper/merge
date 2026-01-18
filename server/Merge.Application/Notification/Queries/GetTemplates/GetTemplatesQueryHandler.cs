using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.DTOs.Notification;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Queries.GetTemplates;


public class GetTemplatesQueryHandler(IDbContext context, IMapper mapper, IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetTemplatesQuery, PagedResult<NotificationTemplateDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;


    public async Task<PagedResult<NotificationTemplateDto>> Handle(GetTemplatesQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize > paginationConfig.MaxPageSize 
            ? paginationConfig.MaxPageSize 
            : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        IQueryable<NotificationTemplate> query = context.Set<NotificationTemplate>()
            .AsNoTracking();

        if (request.Type.HasValue)
        {
            query = query.Where(t => t.Type == request.Type.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var templates = await query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var templateDtos = mapper.Map<List<NotificationTemplateDto>>(templates);

        return new PagedResult<NotificationTemplateDto>
        {
            Items = templateDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
