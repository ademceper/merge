using MediatR;
using Merge.Application.DTOs.User;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.User.Queries.GetUserSessions;

public record GetUserSessionsQuery(
    Guid UserId,
    int Days = 7
) : IRequest<IEnumerable<UserSessionDto>>;
