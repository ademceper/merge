using MediatR;

namespace Merge.Application.Security.Commands.AcknowledgeAlert;

public record AcknowledgeAlertCommand(
    Guid AlertId,
    Guid AcknowledgedByUserId
) : IRequest<bool>;
