using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Marketing.Queries.GetEmailCampaignById;

public class GetEmailCampaignByIdQueryHandler : IRequestHandler<GetEmailCampaignByIdQuery, EmailCampaignDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetEmailCampaignByIdQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<EmailCampaignDto?> Handle(GetEmailCampaignByIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + AsSplitQuery - N+1 query önleme (Cartesian Explosion önleme)
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Template)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return campaign != null ? _mapper.Map<EmailCampaignDto>(campaign) : null;
    }
}
