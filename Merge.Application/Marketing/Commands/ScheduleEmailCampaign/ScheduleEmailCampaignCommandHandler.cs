using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Marketing.Commands.ScheduleEmailCampaign;

public class ScheduleEmailCampaignCommandHandler : IRequestHandler<ScheduleEmailCampaignCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ScheduleEmailCampaignCommandHandler> _logger;

    public ScheduleEmailCampaignCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ScheduleEmailCampaignCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ScheduleEmailCampaignCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Scheduling email campaign. CampaignId: {CampaignId}, ScheduledAt: {ScheduledAt}", 
            request.Id, request.ScheduledAt);

        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (campaign == null)
        {
            _logger.LogWarning("Email campaign not found. CampaignId: {CampaignId}", request.Id);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        campaign.Schedule(request.ScheduledAt);

        // Prepare recipients (EmailCampaignService'deki PrepareCampaignRecipientsAsync mantığı)
        // Bu mantık domain'e taşınabilir veya burada kalabilir
        var subscribers = await GetTargetedSubscribersAsync(campaign.TargetSegment, cancellationToken);
        campaign.StartSending(subscribers.Count);

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Email campaign scheduled successfully. CampaignId: {CampaignId}, ScheduledAt: {ScheduledAt}, Recipients: {Recipients}", 
            request.Id, request.ScheduledAt, subscribers.Count);

        return true;
    }

    private async Task<List<EmailSubscriber>> GetTargetedSubscribersAsync(string segment, CancellationToken cancellationToken)
    {
        IQueryable<EmailSubscriber> query = _context.Set<EmailSubscriber>()
            .Where(s => s.IsSubscribed);

        switch (segment.ToLower())
        {
            case "all":
                break;
            case "active":
                query = query.Where(s => s.EmailsOpened > 0 || s.EmailsClicked > 0);
                break;
            case "inactive":
                query = query.Where(s => s.EmailsOpened == 0 && s.EmailsClicked == 0);
                break;
        }

        return await query.ToListAsync(cancellationToken);
    }
}
