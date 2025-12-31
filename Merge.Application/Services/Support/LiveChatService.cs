using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.Support;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;


namespace Merge.Application.Services.Support;

public class LiveChatService : ILiveChatService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public LiveChatService(ApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<LiveChatSessionDto> CreateSessionAsync(Guid? userId, string? guestName = null, string? guestEmail = null, string? department = null)
    {
        var sessionId = GenerateSessionId();

        var session = new LiveChatSession
        {
            UserId = userId,
            SessionId = sessionId,
            Status = "Waiting",
            GuestName = guestName,
            GuestEmail = guestEmail,
            Department = department,
            StartedAt = DateTime.UtcNow
        };

        await _context.Set<LiveChatSession>().AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return await MapToSessionDto(session);
    }

    public async Task<LiveChatSessionDto?> GetSessionByIdAsync(Guid id)
    {
        var session = await _context.Set<LiveChatSession>()
            .Include(s => s.User)
            .Include(s => s.Agent)
            .Include(s => s.Messages.OrderByDescending(m => m.CreatedAt).Take(50))
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        return session != null ? await MapToSessionDto(session) : null;
    }

    public async Task<LiveChatSessionDto?> GetSessionBySessionIdAsync(string sessionId)
    {
        var session = await _context.Set<LiveChatSession>()
            .Include(s => s.User)
            .Include(s => s.Agent)
            .Include(s => s.Messages.OrderByDescending(m => m.CreatedAt).Take(50))
            .FirstOrDefaultAsync(s => s.SessionId == sessionId && !s.IsDeleted);

        return session != null ? await MapToSessionDto(session) : null;
    }

    public async Task<IEnumerable<LiveChatSessionDto>> GetUserSessionsAsync(Guid userId)
    {
        var sessions = await _context.Set<LiveChatSession>()
            .Include(s => s.User)
            .Include(s => s.Agent)
            .Where(s => s.UserId == userId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        var result = new List<LiveChatSessionDto>();
        foreach (var session in sessions)
        {
            result.Add(await MapToSessionDto(session));
        }
        return result;
    }

    public async Task<IEnumerable<LiveChatSessionDto>> GetAgentSessionsAsync(Guid agentId, string? status = null)
    {
        var query = _context.Set<LiveChatSession>()
            .Include(s => s.User)
            .Include(s => s.Agent)
            .Where(s => s.AgentId == agentId && !s.IsDeleted);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(s => s.Status == status);
        }

        var sessions = await query
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        var result = new List<LiveChatSessionDto>();
        foreach (var session in sessions)
        {
            result.Add(await MapToSessionDto(session));
        }
        return result;
    }

    public async Task<IEnumerable<LiveChatSessionDto>> GetWaitingSessionsAsync()
    {
        var sessions = await _context.Set<LiveChatSession>()
            .Include(s => s.User)
            .Where(s => s.Status == "Waiting" && !s.IsDeleted)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();

        var result = new List<LiveChatSessionDto>();
        foreach (var session in sessions)
        {
            result.Add(await MapToSessionDto(session));
        }
        return result;
    }

    public async Task<bool> AssignAgentAsync(Guid sessionId, Guid agentId)
    {
        var session = await _context.Set<LiveChatSession>()
            .FirstOrDefaultAsync(s => s.Id == sessionId && !s.IsDeleted);

        if (session == null) return false;

        session.AgentId = agentId;
        session.Status = "Active";
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CloseSessionAsync(Guid sessionId)
    {
        var session = await _context.Set<LiveChatSession>()
            .FirstOrDefaultAsync(s => s.Id == sessionId && !s.IsDeleted);

        if (session == null) return false;

        session.Status = "Closed";
        session.ResolvedAt = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<LiveChatMessageDto> SendMessageAsync(Guid sessionId, Guid? senderId, CreateLiveChatMessageDto dto)
    {
        var session = await _context.Set<LiveChatSession>()
            .FirstOrDefaultAsync(s => s.Id == sessionId && !s.IsDeleted);

        if (session == null)
        {
            throw new NotFoundException("Oturum", sessionId);
        }

        string senderType = "User";
        if (senderId.HasValue)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == senderId.Value);
            
            if (user != null)
            {
                // Check if user has admin/manager/support role
                var userRoles = await _context.UserRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                    .ToListAsync();
                
                if (userRoles.Contains("Admin") || userRoles.Contains("Manager") || userRoles.Contains("Support"))
                {
                    senderType = "Agent";
                }
            }
        }

        var message = new LiveChatMessage
        {
            SessionId = sessionId,
            SenderId = senderId,
            SenderType = senderType,
            Content = dto.Content,
            MessageType = dto.MessageType,
            FileUrl = dto.FileUrl,
            FileName = dto.FileName,
            IsInternal = dto.IsInternal
        };

        await _context.Set<LiveChatMessage>().AddAsync(message);
        session.MessageCount++;
        
        // Update unread count
        if (senderType == "User")
        {
            session.UnreadCount++;
        }
        else if (senderType == "Agent")
        {
            session.UnreadCount = 0; // Reset when agent responds
        }

        // Update session status
        if (session.Status == "Waiting" && senderType == "Agent")
        {
            session.Status = "Active";
        }

        await _unitOfWork.SaveChangesAsync();

        return MapToMessageDto(message);
    }

    public async Task<IEnumerable<LiveChatMessageDto>> GetSessionMessagesAsync(Guid sessionId)
    {
        var messages = await _context.Set<LiveChatMessage>()
            .Include(m => m.Sender)
            .Where(m => m.SessionId == sessionId && !m.IsDeleted)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        var result = new List<LiveChatMessageDto>();
        foreach (var message in messages)
        {
            result.Add(MapToMessageDto(message));
        }
        return result;
    }

    public async Task<bool> MarkMessagesAsReadAsync(Guid sessionId, Guid userId)
    {
        var messages = await _context.Set<LiveChatMessage>()
            .Where(m => m.SessionId == sessionId && !m.IsRead && !m.IsDeleted && m.SenderId != userId)
            .ToListAsync();

        foreach (var message in messages)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
        }

        var session = await _context.Set<LiveChatSession>()
            .FirstOrDefaultAsync(s => s.Id == sessionId && !s.IsDeleted);

        if (session != null)
        {
            session.UnreadCount = 0;
        }

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<LiveChatStatsDto> GetChatStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;

        var sessions = await _context.Set<LiveChatSession>()
            .Include(s => s.Agent)
            .Where(s => !s.IsDeleted && s.CreatedAt >= start && s.CreatedAt <= end)
            .ToListAsync();

        var totalSessions = sessions.Count;
        var activeSessions = sessions.Count(s => s.Status == "Active");
        var waitingSessions = sessions.Count(s => s.Status == "Waiting");
        var resolvedSessions = sessions.Count(s => s.Status == "Resolved" || s.Status == "Closed");

        var resolvedSessionsWithTime = sessions
            .Where(s => (s.Status == "Resolved" || s.Status == "Closed") && s.ResolvedAt.HasValue && s.StartedAt.HasValue)
            .ToList();

        var avgResolutionTime = resolvedSessionsWithTime.Any()
            ? resolvedSessionsWithTime.Average(s => (s.ResolvedAt!.Value - s.StartedAt!.Value).TotalMinutes)
            : 0;

        var sessionsByDepartment = sessions
            .Where(s => !string.IsNullOrEmpty(s.Department))
            .GroupBy(s => s.Department!)
            .ToDictionary(g => g.Key, g => g.Count());

        var sessionsByAgent = sessions
            .Where(s => s.AgentId.HasValue)
            .GroupBy(s => s.Agent != null ? $"{s.Agent.FirstName} {s.Agent.LastName}" : "Unknown")
            .ToDictionary(g => g.Key, g => g.Count());

        return new LiveChatStatsDto
        {
            TotalSessions = totalSessions,
            ActiveSessions = activeSessions,
            WaitingSessions = waitingSessions,
            ResolvedSessions = resolvedSessions,
            AverageResolutionTime = (decimal)Math.Round(avgResolutionTime, 2),
            AverageResponseTime = 0, // Can be calculated from first agent message time
            SessionsByDepartment = sessionsByDepartment,
            SessionsByAgent = sessionsByAgent
        };
    }

    private string GenerateSessionId()
    {
        return $"CHAT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }

    private async Task<LiveChatSessionDto> MapToSessionDto(LiveChatSession session)
    {
        await _context.Entry(session)
            .Reference(s => s.User)
            .LoadAsync();
        await _context.Entry(session)
            .Reference(s => s.Agent)
            .LoadAsync();
        await _context.Entry(session)
            .Collection(s => s.Messages)
            .LoadAsync();

        var recentMessages = session.Messages
            .Where(m => !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .Take(10)
            .ToList();

        return new LiveChatSessionDto
        {
            Id = session.Id,
            UserId = session.UserId,
            UserName = session.User != null
                ? $"{session.User.FirstName} {session.User.LastName}"
                : session.GuestName,
            AgentId = session.AgentId,
            AgentName = session.Agent != null
                ? $"{session.Agent.FirstName} {session.Agent.LastName}"
                : null,
            SessionId = session.SessionId,
            Status = session.Status,
            GuestName = session.GuestName,
            GuestEmail = session.GuestEmail,
            StartedAt = session.StartedAt,
            ResolvedAt = session.ResolvedAt,
            MessageCount = session.MessageCount,
            UnreadCount = session.UnreadCount,
            Department = session.Department,
            Priority = session.Priority,
            Tags = !string.IsNullOrEmpty(session.Tags) ? session.Tags.Split(',').ToList() : new List<string>(),
            RecentMessages = recentMessages.Select(MapToMessageDto).ToList(),
            CreatedAt = session.CreatedAt
        };
    }

    private LiveChatMessageDto MapToMessageDto(LiveChatMessage message)
    {
        return new LiveChatMessageDto
        {
            Id = message.Id,
            SessionId = message.SessionId,
            SenderId = message.SenderId,
            SenderName = message.Sender != null
                ? $"{message.Sender.FirstName} {message.Sender.LastName}"
                : null,
            SenderType = message.SenderType,
            Content = message.Content,
            MessageType = message.MessageType,
            IsRead = message.IsRead,
            ReadAt = message.ReadAt,
            FileUrl = message.FileUrl,
            FileName = message.FileName,
            IsInternal = message.IsInternal,
            CreatedAt = message.CreatedAt
        };
    }
}

