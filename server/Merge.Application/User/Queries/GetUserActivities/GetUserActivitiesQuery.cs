using MediatR;
using Merge.Application.DTOs.User;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.User.Queries.GetUserActivities;

public record GetUserActivitiesQuery(
    Guid UserId,
    int Days = 30
) : IRequest<IEnumerable<UserActivityLogDto>>;
