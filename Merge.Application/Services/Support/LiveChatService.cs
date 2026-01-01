using AutoMapper;
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
    private readonly IMapper _mapper;

    public LiveChatService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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

        // ✅ PERFORMANCE: Reload with includes for mapping
        session = await _context.Set<LiveChatSession>()
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Agent)
            .Include(s => s.Messages.OrderByDescending(m => m.CreatedAt).Take(50))
            .FirstOrDefaultAsync(s => s.Id == session.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<LiveChatSessionDto>(session!);
    }

    public async Task<LiveChatSessionDto?> GetSessionByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var session = await _context.Set<LiveChatSession>()
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Agent)
            .Include(s => s.Messages.OrderByDescending(m => m.CreatedAt).Take(50))
            .FirstOrDefaultAsync(s => s.Id == id);

        return session != null ? await MapToSessionDtoAsync(session) : null;
    }

    public async Task<LiveChatSessionDto?> GetSessionBySessionIdAsync(string sessionId)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var session = await _context.Set<LiveChatSession>()
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Agent)
            .Include(s => s.Messages.OrderByDescending(m => m.CreatedAt).Take(50))
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        return session != null ? await MapToSessionDtoAsync(session) : null;
    }

    public async Task<IEnumerable<LiveChatSessionDto>> GetUserSessionsAsync(Guid userId)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var sessions = await _context.Set<LiveChatSession>()
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Agent)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<LiveChatSessionDto>>(sessions);
    }

    public async Task<IEnumerable<LiveChatSessionDto>> GetAgentSessionsAsync(Guid agentId, string? status = null)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var query = _context.Set<LiveChatSession>()
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Agent)
            .Where(s => s.AgentId == agentId);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(s => s.Status == status);
        }

        var sessions = await query
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<LiveChatSessionDto>>(sessions);
    }

    public async Task<IEnumerable<LiveChatSessionDto>> GetWaitingSessionsAsync()
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var sessions = await _context.Set<LiveChatSession>()
            .AsNoTracking()
            .Include(s => s.User)
            .Where(s => s.Status == "Waiting")
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<LiveChatSessionDto>>(sessions);
    }

    public async Task<bool> AssignAgentAsync(Guid sessionId, Guid agentId)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var session = await _context.Set<LiveChatSession>()
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null) return false;

        session.AgentId = agentId;
        session.Status = "Active";
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CloseSessionAsync(Guid sessionId)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var session = await _context.Set<LiveChatSession>()
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null) return false;

        session.Status = "Closed";
        session.ResolvedAt = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<LiveChatMessageDto> SendMessageAsync(Guid sessionId, Guid? senderId, CreateLiveChatMessageDto dto)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var session = await _context.Set<LiveChatSession>()
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
        {
            throw new NotFoundException("Oturum", sessionId);
        }

        // ✅ PERFORMANCE: AsNoTracking for read-only query
        string senderType = "User";
        if (senderId.HasValue)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == senderId.Value);
            
            if (user != null)
            {
                // ✅ PERFORMANCE: Database'de role check yap, memory'de işlem YASAK
                var isAgent = await _context.UserRoles
                    .AsNoTracking()
                    .Where(ur => ur.UserId == user.Id)
                    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                    .AnyAsync(r => r == "Admin" || r == "Manager" || r == "Support");
                
                if (isAgent)
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

        // ✅ PERFORMANCE: Reload with includes for mapping
        message = await _context.Set<LiveChatMessage>()
            .AsNoTracking()
            .Include(m => m.Sender)
            .FirstOrDefaultAsync(m => m.Id == message.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<LiveChatMessageDto>(message!);
    }

    public async Task<IEnumerable<LiveChatMessageDto>> GetSessionMessagesAsync(Guid sessionId)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var messages = await _context.Set<LiveChatMessage>()
            .AsNoTracking()
            .Include(m => m.Sender)
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<LiveChatMessageDto>>(messages);
    }

    public async Task<bool> MarkMessagesAsReadAsync(Guid sessionId, Guid userId)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var messages = await _context.Set<LiveChatMessage>()
            .Where(m => m.SessionId == sessionId && !m.IsRead && m.SenderId != userId)
            .ToListAsync();

        foreach (var message in messages)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
        }

        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var session = await _context.Set<LiveChatSession>()
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session != null)
        {
            session.UnreadCount = 0;
        }

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<LiveChatStatsDto> GetChatStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        // ✅ PERFORMANCE: Database'de aggregations yap, memory'de işlem YASAK
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;

        var query = _context.Set<LiveChatSession>()
            .AsNoTracking()
            .Include(s => s.Agent)
            .Where(s => s.CreatedAt >= start && s.CreatedAt <= end);

        var totalSessions = await query.CountAsync();
        var activeSessions = await query.CountAsync(s => s.Status == "Active");
        var waitingSessions = await query.CountAsync(s => s.Status == "Waiting");
        var resolvedSessions = await query.CountAsync(s => s.Status == "Resolved" || s.Status == "Closed");

        // ✅ PERFORMANCE: Database'de average hesapla
        var resolvedSessionsQuery = query.Where(s => (s.Status == "Resolved" || s.Status == "Closed") && s.ResolvedAt.HasValue && s.StartedAt.HasValue);
        var avgResolutionTime = await resolvedSessionsQuery.AnyAsync()
            ? await resolvedSessionsQuery
                .AverageAsync(s => (double)(s.ResolvedAt!.Value - s.StartedAt!.Value).TotalMinutes)
            : 0;

        // ✅ PERFORMANCE: Database'de grouping yap
        var sessionsByDepartment = await query
            .Where(s => !string.IsNullOrEmpty(s.Department))
            .GroupBy(s => s.Department!)
            .Select(g => new { Department = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Department, x => x.Count);

        var sessionsByAgent = await query
            .Where(s => s.AgentId.HasValue)
            .GroupBy(s => s.Agent != null ? $"{s.Agent.FirstName} {s.Agent.LastName}" : "Unknown")
            .Select(g => new { AgentName = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.AgentName, x => x.Count);

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

    private async Task<LiveChatSessionDto> MapToSessionDtoAsync(LiveChatSession session)
    {
        // ✅ ARCHITECTURE: AutoMapper kullan
        var dto = _mapper.Map<LiveChatSessionDto>(session);

        // ✅ PERFORMANCE: Batch load recent messages if not already loaded
        if (session.Messages == null || session.Messages.Count == 0)
        {
            var recentMessages = await _context.Set<LiveChatMessage>()
                .AsNoTracking()
                .Include(m => m.Sender)
                .Where(m => m.SessionId == session.Id)
                .OrderByDescending(m => m.CreatedAt)
                .Take(10)
                .ToListAsync();
            dto.RecentMessages = _mapper.Map<List<LiveChatMessageDto>>(recentMessages);
        }
        else
        {
            var recentMessages = session.Messages
                .OrderByDescending(m => m.CreatedAt)
                .Take(10)
                .ToList();
            dto.RecentMessages = _mapper.Map<List<LiveChatMessageDto>>(recentMessages);
        }

        return dto;
    }
}

