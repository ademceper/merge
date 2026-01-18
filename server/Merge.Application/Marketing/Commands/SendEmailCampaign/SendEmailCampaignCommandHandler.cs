using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Services.Notification;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.SendEmailCampaign;

public class SendEmailCampaignCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    ILogger<SendEmailCampaignCommandHandler> logger) : IRequestHandler<SendEmailCampaignCommand, bool>
{
    public async Task<bool> Handle(SendEmailCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await context.Set<EmailCampaign>()
            .Include(c => c.Recipients)
            .Include(c => c.Template)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (campaign is null) return false;

        if (campaign.Status == EmailCampaignStatus.Sent)
        {
            throw new BusinessException("Kampanya zaten gönderilmiş.");
        }

        // Prepare recipients if not already done
        if (campaign.Recipients.Count == 0)
        {
            await PrepareCampaignRecipientsAsync(campaign, cancellationToken);
        }

        campaign.StartSending(campaign.TotalRecipients);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Send emails (in production, this would be queued)
        var sentCount = await SendCampaignEmailsAsync(campaign, cancellationToken);

        campaign.MarkAsSent(sentCount);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task PrepareCampaignRecipientsAsync(EmailCampaign campaign, CancellationToken cancellationToken)
    {
        var subscribers = await GetTargetedSubscribersAsync(campaign.TargetSegment, cancellationToken);

        foreach (var subscriber in subscribers)
        {
            var recipient = EmailCampaignRecipient.Create(
                campaignId: campaign.Id,
                subscriberId: subscriber.Id);

            await context.Set<EmailCampaignRecipient>().AddAsync(recipient, cancellationToken);
        }

        // TotalRecipients domain method içinde set ediliyor (StartSending)
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<EmailSubscriber>> GetTargetedSubscribersAsync(string segment, CancellationToken cancellationToken)
    {
        IQueryable<EmailSubscriber> query = context.Set<EmailSubscriber>()
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

    private async Task<int> SendCampaignEmailsAsync(EmailCampaign campaign, CancellationToken cancellationToken)
    {
        var recipients = await context.Set<EmailCampaignRecipient>()
            .Include(r => r.Subscriber)
            .Where(r => r.CampaignId == campaign.Id && r.Status == EmailRecipientStatus.Pending)
            .ToListAsync(cancellationToken);

        var content = !string.IsNullOrEmpty(campaign.Content)
            ? campaign.Content
            : campaign.Template?.HtmlContent ?? string.Empty;

        int sentCount = 0;
        foreach (var recipient in recipients)
        {
            try
            {
                await emailService.SendEmailAsync(
                    recipient.Subscriber.Email,
                    campaign.Subject,
                    content,
                    true,
                    cancellationToken
                );

                recipient.MarkAsSent();
                sentCount++;
                
                var subscriber = recipient.Subscriber;
                subscriber.RecordEmailSent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Email gönderilemedi. CampaignId: {CampaignId}, SubscriberId: {SubscriberId}",
                    campaign.Id, recipient.SubscriberId);
                recipient.MarkAsFailed(ex.Message);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return sentCount;
    }
}
