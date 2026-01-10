using MediatR;

namespace Merge.Application.Marketing.Commands.RecordEmailClick;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RecordEmailClickCommand(Guid CampaignId, Guid SubscriberId) : IRequest<Unit>;
