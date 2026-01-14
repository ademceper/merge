using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetTicketMessages;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetTicketMessagesQuery(
    Guid TicketId,
    bool IncludeInternal = false
) : IRequest<IEnumerable<TicketMessageDto>>;
