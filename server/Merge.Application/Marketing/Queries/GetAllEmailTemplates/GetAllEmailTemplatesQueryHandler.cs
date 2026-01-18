using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetAllEmailTemplates;

public class GetAllEmailTemplatesQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetAllEmailTemplatesQuery, PagedResult<EmailTemplateDto>>
{
    public async Task<PagedResult<EmailTemplateDto>> Handle(GetAllEmailTemplatesQuery request, CancellationToken cancellationToken)
    {
        IQueryable<EmailTemplate> query = context.Set<EmailTemplate>()
            .AsNoTracking()
            .Where(t => t.IsActive);

        if (!string.IsNullOrEmpty(request.Type))
        {
            if (Enum.TryParse<EmailTemplateType>(request.Type, true, out var typeEnum))
            {
                query = query.Where(t => t.Type == typeEnum);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var templates = await query
            .OrderBy(t => t.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<EmailTemplateDto>
        {
            Items = mapper.Map<List<EmailTemplateDto>>(templates),
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
