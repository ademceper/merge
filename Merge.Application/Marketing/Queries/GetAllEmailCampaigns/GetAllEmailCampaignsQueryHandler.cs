using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Marketing.Queries.GetAllEmailCampaigns;

public class GetAllEmailCampaignsQueryHandler : IRequestHandler<GetAllEmailCampaignsQuery, PagedResult<EmailCampaignDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetAllEmailCampaignsQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<EmailCampaignDto>> Handle(GetAllEmailCampaignsQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        IQueryable<EmailCampaign> query = _context.Set<EmailCampaign>()
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

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return new PagedResult<EmailCampaignDto>
        {
            Items = _mapper.Map<List<EmailCampaignDto>>(campaigns),
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
