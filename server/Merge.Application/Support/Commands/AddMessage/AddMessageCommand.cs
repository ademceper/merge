using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Commands.AddMessage;

public record AddMessageCommand(
    Guid UserId,
    Guid TicketId,
    string Message,
    bool IsStaffResponse = false,
    bool IsInternal = false
) : IRequest<TicketMessageDto>;
