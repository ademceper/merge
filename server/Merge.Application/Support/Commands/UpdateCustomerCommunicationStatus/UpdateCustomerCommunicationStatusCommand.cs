using MediatR;

namespace Merge.Application.Support.Commands.UpdateCustomerCommunicationStatus;

public record UpdateCustomerCommunicationStatusCommand(
    Guid CommunicationId,
    string Status,
    DateTime? DeliveredAt = null,
    DateTime? ReadAt = null
) : IRequest<bool>;
