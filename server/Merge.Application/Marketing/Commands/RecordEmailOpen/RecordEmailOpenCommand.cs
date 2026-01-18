using MediatR;

namespace Merge.Application.Marketing.Commands.RecordEmailOpen;

public record RecordEmailOpenCommand(Guid CampaignId, Guid SubscriberId) : IRequest<Unit>;
