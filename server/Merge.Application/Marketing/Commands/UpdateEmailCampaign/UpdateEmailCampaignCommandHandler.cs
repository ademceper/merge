using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using System.Text.Json;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.UpdateEmailCampaign;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class UpdateEmailCampaignCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateEmailCampaignCommandHandler> logger) : IRequestHandler<UpdateEmailCampaignCommand, EmailCampaignDto>
{
    public async Task<EmailCampaignDto> Handle(UpdateEmailCampaignCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await context.Set<EmailCampaign>()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (campaign == null)
        {
            throw new NotFoundException("Kampanya", request.Id);
        }

        if (campaign.Status != Merge.Domain.Enums.EmailCampaignStatus.Draft)
        {
            throw new BusinessException("Sadece taslak kampanyalar güncellenebilir.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        campaign.UpdateDetails(
            request.Name ?? campaign.Name,
            request.Subject ?? campaign.Subject,
            request.FromName ?? campaign.FromName,
            request.FromEmail ?? campaign.FromEmail,
            request.ReplyToEmail ?? campaign.ReplyToEmail,
            request.Content ?? campaign.Content);

        if (request.TemplateId.HasValue)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
            campaign.SetTemplateId(request.TemplateId.Value);
        }

        if (request.ScheduledAt.HasValue && request.ScheduledAt != campaign.ScheduledAt)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
            campaign.Schedule(request.ScheduledAt.Value);
        }

        if (!string.IsNullOrEmpty(request.TargetSegment) && request.TargetSegment != campaign.TargetSegment)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
            campaign.SetTargetSegment(request.TargetSegment);
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with includes in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + AsSplitQuery + Removed manual !c.IsDeleted (Global Query Filter)
        var updatedCampaign = await context.Set<EmailCampaign>()
            .AsNoTracking()
            .Include(c => c.Template)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<EmailCampaignDto>(updatedCampaign!);
    }
}
