using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetUserCommunicationHistory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetUserCommunicationHistoryQuery(
    Guid UserId
) : IRequest<CommunicationHistoryDto>;
