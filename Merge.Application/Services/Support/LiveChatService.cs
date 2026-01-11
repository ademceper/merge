using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces.Support;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Common;
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
    private readonly SupportSettings _settings;

    public LiveChatService(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<LiveChatService> logger,
        IOptions<SupportSettings> settings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _settings = settings.Value;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<LiveChatSessionDto> CreateSessionAsync(Guid? userId, string? guestName = null, string? guestEmail = null, string? department = null, CancellationToken cancellationToken = default)
    {
        var sessionId = GenerateSessionId();

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var session = LiveChatSession.Create(
            sessionId,
            userId,
            guestName,
            guestEmail,
            department,
            null, // IP address will be set by controller if needed
            null); // User agent will be set by controller if needed

        await _context.Set<LiveChatSession>().AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with includes for mapping
        session = await _context.Set<LiveChatSession>()
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Agent)
            // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma
            .Include(s => s.Messages.OrderByDescending(m => m.CreatedAt).Take(_settings.MaxRecentChatMessages))
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
            // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma
            .Include(s => s.Messages.OrderByDescending(m => m.CreatedAt).Take(_settings.MaxRecentChatMessages))
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
            // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma
            .Include(s => s.Messages.OrderByDescending(m => m.CreatedAt).Take(_settings.MaxRecentChatMessages))
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

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        session.AssignAgent(agentId);
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

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        session.Close();
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

        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Service layer validation
        Guard.AgainstLength(dto.Content, _settings.MaxLiveChatMessageLength, nameof(dto.Content));

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var message = LiveChatMessage.Create(
            sessionId,
            session.SessionId,
            senderId,
            senderType,
            dto.Content,
            dto.MessageType,
            dto.FileUrl,
            dto.FileName,
            dto.IsInternal);

        await _context.Set<LiveChatMessage>().AddAsync(message, cancellationToken);
        
        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        session.AddMessage(senderType);

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

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        foreach (var message in messages)
        {
            message.MarkAsRead();
        }

        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var session = await _context.Set<LiveChatSession>()
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

        if (session != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            session.MarkMessagesAsRead();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<LiveChatStatsDto> GetChatStatsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Database'de aggregations yap, memory'de işlem YASAK
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma
        var start = startDate ?? DateTime.UtcNow.AddDays(-_settings.DefaultStatsPeriodDays);
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
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma
        var datePart = DateTime.UtcNow.ToString(_settings.ChatSessionIdDateFormat);
        var guidPart = Guid.NewGuid().ToString().Substring(0, _settings.ChatSessionIdGuidLength).ToUpper();
        return $"{_settings.ChatSessionIdPrefix}{datePart}-{guidPart}";
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
                // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma
                .Take(_settings.DashboardRecentTicketsCount)
                .ToListAsync(cancellationToken);
            dto.RecentMessages = _mapper.Map<List<LiveChatMessageDto>>(recentMessages);
        }
        else
        {
            var recentMessages = session.Messages
                .OrderByDescending(m => m.CreatedAt)
                // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma
                .Take(_settings.DashboardRecentTicketsCount)
                .ToList();
            dto.RecentMessages = _mapper.Map<List<LiveChatMessageDto>>(recentMessages);
        }

        return dto;
    }
}

