using MediatR;

namespace Merge.Application.Marketing.Commands.RecordEmailOpen;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RecordEmailOpenCommand(Guid CampaignId, Guid SubscriberId) : IRequest<Unit>;
