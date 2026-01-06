using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Support;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;
using UserRole = Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>;


namespace Merge.Application.Services.Support;

public class LiveChatService : ILiveChatService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<LiveChatService> _logger;

    public LiveChatService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<LiveChatService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<LiveChatSessionDto> CreateSessionAsync(Guid? userId, string? guestName = null, string? guestEmail = null, string? department = null, CancellationToken cancellationToken = default)
    {
        var sessionId = GenerateSessionId();

        var session = new LiveChatSession
        {
            UserId = userId,
            SessionId = sessionId,
            Status = ChatSessionStatus.Waiting,
            GuestName = guestName,
            GuestEmail = guestEmail,
            Department = department,
            StartedAt = DateTime.UtcNow
        };

        await _context.Set<LiveChatSession>().AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with includes for mapping
        session = await _context.Set<LiveChatSession>()
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Agent)
            .Include(s => s.Messages.OrderByDescending(m => m.CreatedAt).Take(50))
            .FirstOrDefaultAsync(s => s.Id == session.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<LiveChatSessionDto>(session!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<LiveChatSessionDto?> GetSessionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var session = await _context.Set<LiveChatSession>()
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Agent)
            .Include(s => s.Messages.OrderByDescending(m => m.CreatedAt).Take(50))
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        return session != null ? await MapToSessionDtoAsync(session, cancellationToken) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<LiveChatSessionDto?> GetSessionBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var session = await _context.Set<LiveChatSession>()
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Agent)
            .Include(s => s.Messages.OrderByDescending(m => m.CreatedAt).Take(50))
            .FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);

        return session != null ? await MapToSessionDtoAsync(session, cancellationToken) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<LiveChatSessionDto>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var sessions = await _context.Set<LiveChatSession>()
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Agent)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<LiveChatSessionDto>>(sessions);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<LiveChatSessionDto>> GetAgentSessionsAsync(Guid agentId, string? status = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        IQueryable<LiveChatSession> query = _context.Set<LiveChatSession>()
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Agent)
            .Where(s => s.AgentId == agentId);

        // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<ChatSessionStatus>(status, true, out var statusEnum))
            {
                query = query.Where(s => s.Status == statusEnum);
            }
        }

        var sessions = await query
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<LiveChatSessionDto>>(sessions);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<LiveChatSessionDto>> GetWaitingSessionsAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var sessions = await _context.Set<LiveChatSession>()
            .AsNoTracking()
            .Include(s => s.User)
            .Where(s => s.Status == ChatSessionStatus.Waiting)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<LiveChatSessionDto>>(sessions);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> AssignAgentAsync(Guid sessionId, Guid agentId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var session = await _context.Set<LiveChatSession>()
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

        if (session == null) return false;

        session.AgentId = agentId;
        session.Status = ChatSessionStatus.Active;
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> CloseSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var session = await _context.Set<LiveChatSession>()
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

        if (session == null) return false;

        session.Status = ChatSessionStatus.Closed;
        session.ResolvedAt = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<LiveChatMessageDto> SendMessageAsync(Guid sessionId, Guid? senderId, CreateLiveChatMessageDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var session = await _context.Set<LiveChatSession>()
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

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
                .FirstOrDefaultAsync(u => u.Id == senderId.Value, cancellationToken);

            if (user != null)
            {
                // ✅ PERFORMANCE: Database'de role check yap, memory'de işlem YASAK
                // ✅ Identity framework'ün Role ve UserRole entity'leri IDbContext üzerinden erişiliyor
                var isAgent = await _context.UserRoles
                    .AsNoTracking()
                    .Where(ur => ur.UserId == user.Id)
                    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                    .AnyAsync(r => r == "Admin" || r == "Manager" || r == "Support", cancellationToken);

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

        await _context.Set<LiveChatMessage>().AddAsync(message, cancellationToken);
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
        if (session.Status == ChatSessionStatus.Waiting && senderType == "Agent")
        {
            session.Status = ChatSessionStatus.Active;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with includes for mapping
        message = await _context.Set<LiveChatMessage>()
            .AsNoTracking()
            .Include(m => m.Sender)
            .FirstOrDefaultAsync(m => m.Id == message.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<LiveChatMessageDto>(message!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<LiveChatMessageDto>> GetSessionMessagesAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var messages = await _context.Set<LiveChatMessage>()
            .AsNoTracking()
            .Include(m => m.Sender)
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<LiveChatMessageDto>>(messages);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> MarkMessagesAsReadAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var messages = await _context.Set<LiveChatMessage>()
            .Where(m => m.SessionId == sessionId && !m.IsRead && m.SenderId != userId)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
        }

        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var session = await _context.Set<LiveChatSession>()
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

        if (session != null)
        {
            session.UnreadCount = 0;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<LiveChatStatsDto> GetChatStatsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Database'de aggregations yap, memory'de işlem YASAK
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;

        IQueryable<LiveChatSession> query = _context.Set<LiveChatSession>()
            .AsNoTracking()
            .Include(s => s.Agent)
            .Where(s => s.CreatedAt >= start && s.CreatedAt <= end);

        var totalSessions = await query.CountAsync(cancellationToken);
        var activeSessions = await query.CountAsync(s => s.Status == ChatSessionStatus.Active, cancellationToken);
        var waitingSessions = await query.CountAsync(s => s.Status == ChatSessionStatus.Waiting, cancellationToken);
        var resolvedSessions = await query.CountAsync(s => s.Status == ChatSessionStatus.Resolved || s.Status == ChatSessionStatus.Closed, cancellationToken);

        // ✅ PERFORMANCE: Database'de average hesapla
        var resolvedSessionsQuery = query.Where(s => (s.Status == ChatSessionStatus.Resolved || s.Status == ChatSessionStatus.Closed) && s.ResolvedAt.HasValue && s.StartedAt.HasValue);
        var avgResolutionTime = await resolvedSessionsQuery.AnyAsync(cancellationToken)
            ? await resolvedSessionsQuery
                .AverageAsync(s => (double)(s.ResolvedAt!.Value - s.StartedAt!.Value).TotalMinutes, cancellationToken)
            : 0;

        // ✅ PERFORMANCE: Database'de grouping yap
        var sessionsByDepartment = await query
            .Where(s => !string.IsNullOrEmpty(s.Department))
            .GroupBy(s => s.Department!)
            .Select(g => new { Department = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Department, x => x.Count, cancellationToken);

        var sessionsByAgent = await query
            .Where(s => s.AgentId.HasValue)
            .GroupBy(s => s.Agent != null ? $"{s.Agent.FirstName} {s.Agent.LastName}" : "Unknown")
            .Select(g => new { AgentName = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.AgentName, x => x.Count, cancellationToken);

        return new LiveChatStatsDto(
            totalSessions,
            activeSessions,
            waitingSessions,
            resolvedSessions,
            (decimal)Math.Round(avgResolutionTime, 2),
            0, // AverageResponseTime - Can be calculated from first agent message time
            sessionsByDepartment,
            sessionsByAgent
        );
    }

    private string GenerateSessionId()
    {
        return $"CHAT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    private async Task<LiveChatSessionDto> MapToSessionDtoAsync(LiveChatSession session, CancellationToken cancellationToken = default)
    {
        // ✅ ARCHITECTURE: AutoMapper kullan
        var dto = _mapper.Map<LiveChatSessionDto>(session);

        // ✅ PERFORMANCE: Batch load recent messages if not already loaded
        if (session.Messages == null || session.Messages.Count == 0)
        {
            // ✅ PERFORMANCE: AsNoTracking + Removed manual !m.IsDeleted (Global Query Filter)
            var recentMessages = await _context.Set<LiveChatMessage>()
                .AsNoTracking()
                .Include(m => m.Sender)
                .Where(m => m.SessionId == session.Id)
                .OrderByDescending(m => m.CreatedAt)
                .Take(10)
                .ToListAsync(cancellationToken);
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

