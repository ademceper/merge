using MediatR;
using Merge.Application.DTOs.User;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.User.Queries.SearchActivities;

public record SearchActivitiesQuery(ActivityFilterDto Filter) : IRequest<IEnumerable<UserActivityLogDto>>;
