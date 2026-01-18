using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Services.Notification;
using Merge.Application.Exceptions;
using Merge.Application.Common;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using static Merge.Application.Common.LogMasking;

namespace Merge.Application.Marketing.Commands.SendTestEmail;

public class SendTestEmailCommandHandler(
    IDbContext context,
    IEmailService emailService,
    ILogger<SendTestEmailCommandHandler> logger) : IRequestHandler<SendTestEmailCommand>
{
    public async Task Handle(SendTestEmailCommand request, CancellationToken cancellationToken)
    {
        var campaign = await context.Set<EmailCampaign>()
            .AsNoTracking()
            .Include(c => c.Template)
            .FirstOrDefaultAsync(c => c.Id == request.CampaignId, cancellationToken);

        if (campaign is null)
        {
            throw new NotFoundException("Kampanya", request.CampaignId);
        }

        var content = !string.IsNullOrEmpty(campaign.Content)
            ? campaign.Content
            : campaign.Template?.HtmlContent ?? string.Empty;

        await emailService.SendEmailAsync(
            request.TestEmail,
            campaign.Subject + " [TEST]",
            content,
            true,
            cancellationToken
        );

        logger.LogInformation("Test email gönderildi. CampaignId: {CampaignId}, TestEmail: {MaskedEmail}",
            request.CampaignId, MaskEmail(request.TestEmail));
    }
}
