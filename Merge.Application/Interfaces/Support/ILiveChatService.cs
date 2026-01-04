using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Support;

public interface ILiveChatService
{
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    Task<LiveChatSessionDto> CreateSessionAsync(Guid? userId, string? guestName = null, string? guestEmail = null, string? department = null, CancellationToken cancellationToken = default);
    Task<LiveChatSessionDto?> GetSessionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LiveChatSessionDto?> GetSessionBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LiveChatSessionDto>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LiveChatSessionDto>> GetAgentSessionsAsync(Guid agentId, string? status = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<LiveChatSessionDto>> GetWaitingSessionsAsync(CancellationToken cancellationToken = default);
    Task<bool> AssignAgentAsync(Guid sessionId, Guid agentId, CancellationToken cancellationToken = default);
    Task<bool> CloseSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<LiveChatMessageDto> SendMessageAsync(Guid sessionId, Guid? senderId, CreateLiveChatMessageDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<LiveChatMessageDto>> GetSessionMessagesAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<bool> MarkMessagesAsReadAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken = default);
    Task<LiveChatStatsDto> GetChatStatsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}

