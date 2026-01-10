using MediatR;

namespace Merge.Application.Marketing.Commands.SendTestEmail;

public record SendTestEmailCommand(
    Guid CampaignId,
    string TestEmail) : IRequest;
