using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Commands.AddMessage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record AddMessageCommand(
    Guid UserId,
    Guid TicketId,
    string Message,
    bool IsStaffResponse = false,
    bool IsInternal = false
) : IRequest<TicketMessageDto>;
