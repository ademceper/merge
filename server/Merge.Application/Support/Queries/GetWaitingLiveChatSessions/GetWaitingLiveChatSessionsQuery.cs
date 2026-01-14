using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Support.Queries.GetWaitingLiveChatSessions;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetWaitingLiveChatSessionsQuery() : IRequest<IEnumerable<LiveChatSessionDto>>;
