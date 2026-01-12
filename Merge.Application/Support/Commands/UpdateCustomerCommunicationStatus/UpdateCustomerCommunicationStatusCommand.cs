using MediatR;

namespace Merge.Application.Support.Commands.UpdateCustomerCommunicationStatus;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateCustomerCommunicationStatusCommand(
    Guid CommunicationId,
    string Status,
    DateTime? DeliveredAt = null,
    DateTime? ReadAt = null
) : IRequest<bool>;
