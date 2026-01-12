using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Commands.CreateTicket;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateTicketCommand(
    Guid UserId,
    string Category,
    string Priority,
    string Subject,
    string Description,
    Guid? OrderId = null,
    Guid? ProductId = null
) : IRequest<SupportTicketDto>;
