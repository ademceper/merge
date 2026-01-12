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

public class UpdateEmailCampaignCommandHandler : IRequestHandler<UpdateEmailCampaignCommand, EmailCampaignDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateEmailCampaignCommandHandler> _logger;

    public UpdateEmailCampaignCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateEmailCampaignCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<EmailCampaignDto> Handle(UpdateEmailCampaignCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
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
            // TemplateId için reflection veya property set gerekli (domain method'da yok)
            var campaignType = typeof(EmailCampaign);
            var templateIdProperty = campaignType.GetProperty("TemplateId");
            if (templateIdProperty != null && templateIdProperty.CanWrite)
            {
                templateIdProperty.SetValue(campaign, request.TemplateId.Value);
            }
        }

        if (request.ScheduledAt.HasValue && request.ScheduledAt != campaign.ScheduledAt)
        {
            campaign.Schedule(request.ScheduledAt.Value);
        }

        if (!string.IsNullOrEmpty(request.TargetSegment) && request.TargetSegment != campaign.TargetSegment)
        {
            var targetSegmentProperty = typeof(EmailCampaign).GetProperty("TargetSegment");
            if (targetSegmentProperty != null && targetSegmentProperty.CanWrite)
            {
                targetSegmentProperty.SetValue(campaign, request.TargetSegment);
            }
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with includes in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + AsSplitQuery + Removed manual !c.IsDeleted (Global Query Filter)
        var updatedCampaign = await _context.Set<EmailCampaign>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Template)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<EmailCampaignDto>(updatedCampaign!);
    }
}
