using MediatR;

namespace Merge.Application.Security.Commands.AcknowledgeAlert;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record AcknowledgeAlertCommand(
    Guid AlertId,
    Guid AcknowledgedByUserId
) : IRequest<bool>;
