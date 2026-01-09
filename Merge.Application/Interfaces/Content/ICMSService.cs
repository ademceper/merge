using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;
using Merge.Application.Common;

namespace Merge.Application.Interfaces.Content;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
public interface ICMSService
{
    [Obsolete("Use CreateCMSPageCommand via MediatR instead")]
    Task<CMSPageDto> CreatePageAsync(Guid? authorId, object dto, CancellationToken cancellationToken = default);
    Task<CMSPageDto?> GetPageByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CMSPageDto?> GetPageBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<CMSPageDto?> GetHomePageAsync(CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination eklendi (ZORUNLU)
    Task<PagedResult<CMSPageDto>> GetAllPagesAsync(string? status = null, bool? showInMenu = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<CMSPageDto>> GetMenuPagesAsync(CancellationToken cancellationToken = default);
    [Obsolete("Use UpdateCMSPageCommand via MediatR instead")]
    Task<bool> UpdatePageAsync(Guid id, object dto, CancellationToken cancellationToken = default);
    Task<bool> DeletePageAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> PublishPageAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SetHomePageAsync(Guid id, CancellationToken cancellationToken = default);
}

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


