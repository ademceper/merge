using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetAllEmailCampaigns;

public class GetAllEmailCampaignsQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetAllEmailCampaignsQuery, PagedResult<EmailCampaignDto>>
{
    public async Task<PagedResult<EmailCampaignDto>> Handle(GetAllEmailCampaignsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<EmailCampaign> query = context.Set<EmailCampaign>()
            .AsNoTracking();

        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<EmailCampaignStatus>(request.Status, true, out var statusEnum))
            {
                query = query.Where(c => c.Status == statusEnum);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var campaigns = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<EmailCampaignDto>
        {
            Items = mapper.Map<List<EmailCampaignDto>>(campaigns),
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
