using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Configuration;
using Merge.Application.DTOs.User;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.User.Queries.GetUserSessions;

public class GetUserSessionsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetUserSessionsQueryHandler> logger, IOptions<UserSettings> userSettings) : IRequestHandler<GetUserSessionsQuery, IEnumerable<UserSessionDto>>
{
    public async Task<IEnumerable<UserSessionDto>> Handle(GetUserSessionsQuery request, CancellationToken cancellationToken)
    {

        logger.LogInformation("Retrieving user sessions for user: {UserId} for last {Days} days", request.UserId, request.Days);
        var days = request.Days;
        if (days > userSettings.Value.Activity.MaxSessionDays) days = userSettings.Value.Activity.MaxSessionDays;
        if (days < 1) days = userSettings.Value.Activity.DefaultSessionDays;

        var startDate = DateTime.UtcNow.AddDays(-days);

        var activities =         // ✅ PERFORMANCE: AsNoTracking
        await context.Set<UserActivityLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => a.UserId == request.UserId && a.CreatedAt >= startDate)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        var sessions = new List<UserSessionDto>(activities.Count > 0 ? activities.Count / userSettings.Value.Activity.AverageActivitiesPerSession : 1);
        List<UserActivityLog> currentSession = [];
        var sessionTimeout = TimeSpan.FromMinutes(userSettings.Value.Activity.SessionTimeoutMinutes);

        foreach (var activity in activities)
        {
            if (currentSession.Any() &&
                (activity.CreatedAt - currentSession.Last().CreatedAt) > sessionTimeout)
            {
                sessions.Add(CreateSessionDto(currentSession));
                currentSession = [];
            }

            currentSession.Add(activity);
        }

        if (currentSession.Any())
        {
            sessions.Add(CreateSessionDto(currentSession));
        }

        logger.LogInformation("Found {Count} sessions for user: {UserId}", sessions.Count, request.UserId);

        return sessions;
    }

    private UserSessionDto CreateSessionDto(List<UserActivityLog> activities)
    {
        if (activities is null || activities.Count == 0)
        {
            throw new ArgumentException("Activities list cannot be empty", nameof(activities));
        }
        
        var first = activities.First();
        var last = activities.Last();

        return new UserSessionDto
        {
            UserId = first.UserId,
            UserEmail = first.User?.Email ?? "Anonymous",
            SessionStart = first.CreatedAt,
            SessionEnd = last.CreatedAt,
            DurationMinutes = (int)(last.CreatedAt - first.CreatedAt).TotalMinutes,
            ActivitiesCount = activities.Count,
            Activities = mapper.Map<List<UserActivityLogDto>>(activities)
        };
    }
}
