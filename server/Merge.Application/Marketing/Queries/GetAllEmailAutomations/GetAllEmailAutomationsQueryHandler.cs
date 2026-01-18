using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetAllEmailAutomations;

public class GetAllEmailAutomationsQueryHandler(
    IDbContext context,
    IMapper mapper) : IRequestHandler<GetAllEmailAutomationsQuery, PagedResult<EmailAutomationDto>>
{

    public async Task<PagedResult<EmailAutomationDto>> Handle(GetAllEmailAutomationsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Set<EmailAutomation>()
            .AsNoTracking()
            .Include(a => a.Template)
            .OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var automations = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<EmailAutomationDto>
        {
            Items = mapper.Map<List<EmailAutomationDto>>(automations),
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
