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

public class GetEmailCampaignByIdQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetEmailCampaignByIdQuery, EmailCampaignDto?>
{
    public async Task<EmailCampaignDto?> Handle(GetEmailCampaignByIdQuery request, CancellationToken cancellationToken)
    {
        var campaign = await context.Set<EmailCampaign>()
            .AsNoTracking()
            .Include(c => c.Template)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        return campaign != null ? mapper.Map<EmailCampaignDto>(campaign) : null;
    }
}
