using MediatR;

namespace Merge.Application.Marketing.Commands.RecordEmailClick;

public record RecordEmailClickCommand(Guid CampaignId, Guid SubscriberId) : IRequest<Unit>;
