using MediatR;
using Merge.Application.DTOs.User;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.User.Queries.GetActivityStats;

public record GetActivityStatsQuery(int Days = 30) : IRequest<ActivityStatsDto>;
