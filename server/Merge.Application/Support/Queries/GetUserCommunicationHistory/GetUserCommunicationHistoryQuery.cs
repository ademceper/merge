using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetUserCommunicationHistory;

public record GetUserCommunicationHistoryQuery(
    Guid UserId
) : IRequest<CommunicationHistoryDto>;
