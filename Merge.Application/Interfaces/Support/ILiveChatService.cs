using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Interfaces.Support;

public interface ILiveChatService
{
    Task<LiveChatSessionDto> CreateSessionAsync(Guid? userId, string? guestName = null, string? guestEmail = null, string? department = null);
    Task<LiveChatSessionDto?> GetSessionByIdAsync(Guid id);
    Task<LiveChatSessionDto?> GetSessionBySessionIdAsync(string sessionId);
    Task<IEnumerable<LiveChatSessionDto>> GetUserSessionsAsync(Guid userId);
    Task<IEnumerable<LiveChatSessionDto>> GetAgentSessionsAsync(Guid agentId, string? status = null);
    Task<IEnumerable<LiveChatSessionDto>> GetWaitingSessionsAsync();
    Task<bool> AssignAgentAsync(Guid sessionId, Guid agentId);
    Task<bool> CloseSessionAsync(Guid sessionId);
    Task<LiveChatMessageDto> SendMessageAsync(Guid sessionId, Guid? senderId, CreateLiveChatMessageDto dto);
    Task<IEnumerable<LiveChatMessageDto>> GetSessionMessagesAsync(Guid sessionId);
    Task<bool> MarkMessagesAsReadAsync(Guid sessionId, Guid userId);
    Task<LiveChatStatsDto> GetChatStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
}

