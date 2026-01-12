using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Commands.CreateCustomerCommunication;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateCustomerCommunicationCommand(
    Guid UserId,
    string CommunicationType,
    string Channel,
    string Subject,
    string Content,
    string Direction = "Outbound",
    Guid? RelatedEntityId = null,
    string? RelatedEntityType = null,
    Guid? SentByUserId = null,
    string? RecipientEmail = null,
    string? RecipientPhone = null,
    CustomerCommunicationSettingsDto? Metadata = null
) : IRequest<CustomerCommunicationDto>;
