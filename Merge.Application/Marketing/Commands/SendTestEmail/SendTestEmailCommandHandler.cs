using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Services.Notification;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.SendTestEmail;

public class SendTestEmailCommandHandler : IRequestHandler<SendTestEmailCommand>
{
    private readonly IDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendTestEmailCommandHandler> _logger;

    public SendTestEmailCommandHandler(
        IDbContext context,
        IEmailService emailService,
        ILogger<SendTestEmailCommandHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(SendTestEmailCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + AsSplitQuery + Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Template)
            .FirstOrDefaultAsync(c => c.Id == request.CampaignId, cancellationToken);

        if (campaign == null)
        {
            throw new NotFoundException("Kampanya", request.CampaignId);
        }

        var content = !string.IsNullOrEmpty(campaign.Content)
            ? campaign.Content
            : campaign.Template?.HtmlContent ?? string.Empty;

        await _emailService.SendEmailAsync(
            request.TestEmail,
            campaign.Subject + " [TEST]",
            content,
            true,
            cancellationToken
        );

        _logger.LogInformation("Test email gönderildi. CampaignId: {CampaignId}, TestEmail: {TestEmail}",
            request.CampaignId, request.TestEmail);
    }
}
