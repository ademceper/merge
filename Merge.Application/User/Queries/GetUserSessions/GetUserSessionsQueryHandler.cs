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
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.User.Queries.GetUserSessions;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetUserSessionsQueryHandler : IRequestHandler<GetUserSessionsQuery, IEnumerable<UserSessionDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserSessionsQueryHandler> _logger;
    private readonly UserSettings _userSettings;

    public GetUserSessionsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetUserSessionsQueryHandler> logger,
        IOptions<UserSettings> userSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _userSettings = userSettings.Value;
    }

    public async Task<IEnumerable<UserSessionDto>> Handle(GetUserSessionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving user sessions for user: {UserId} for last {Days} days", request.UserId, request.Days);

        // ✅ BOLUM 12.0: Magic numbers configuration'dan alınıyor
        var days = request.Days;
        if (days > _userSettings.Activity.MaxSessionDays) days = _userSettings.Activity.MaxSessionDays;
        if (days < 1) days = _userSettings.Activity.DefaultSessionDays;

        var startDate = DateTime.UtcNow.AddDays(-days);

        var activities = await _context.Set<UserActivityLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => a.UserId == request.UserId && a.CreatedAt >= startDate)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        var sessions = new List<UserSessionDto>(activities.Count > 0 ? activities.Count / 10 : 1);
        var currentSession = new List<UserActivityLog>();
        var sessionTimeout = TimeSpan.FromMinutes(_userSettings.Activity.SessionTimeoutMinutes);

        foreach (var activity in activities)
        {
            if (currentSession.Any() &&
                (activity.CreatedAt - currentSession.Last().CreatedAt) > sessionTimeout)
            {
                sessions.Add(CreateSessionDto(currentSession));
                currentSession = new List<UserActivityLog>();
            }

            currentSession.Add(activity);
        }

        if (currentSession.Any())
        {
            sessions.Add(CreateSessionDto(currentSession));
        }

        _logger.LogInformation("Found {Count} sessions for user: {UserId}", sessions.Count, request.UserId);

        return sessions;
    }

    private UserSessionDto CreateSessionDto(List<UserActivityLog> activities)
    {
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
            Activities = _mapper.Map<List<UserActivityLogDto>>(activities)
        };
    }
}
