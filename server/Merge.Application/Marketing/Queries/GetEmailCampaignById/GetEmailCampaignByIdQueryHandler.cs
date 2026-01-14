using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetEmailCampaignById;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetEmailCampaignByIdQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetEmailCampaignByIdQuery, EmailCampaignDto?>
{
    public async Task<EmailCampaignDto?> Handle(GetEmailCampaignByIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + AsSplitQuery - N+1 query önleme (Cartesian Explosion önleme)
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await context.Set<EmailCampaign>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Template)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return campaign != null ? mapper.Map<EmailCampaignDto>(campaign) : null;
    }
}
