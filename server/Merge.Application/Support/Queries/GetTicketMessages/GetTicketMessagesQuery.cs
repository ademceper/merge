using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetTicketMessages;

public record GetTicketMessagesQuery(
    Guid TicketId,
    bool IncludeInternal = false
) : IRequest<IEnumerable<TicketMessageDto>>;
