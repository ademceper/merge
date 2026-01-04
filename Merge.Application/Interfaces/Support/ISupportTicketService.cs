using Merge.Application.DTOs.Support;
using Merge.Application.Common;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
namespace Merge.Application.Interfaces.Support;

public interface ISupportTicketService
{
    // Ticket management
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    Task<SupportTicketDto> CreateTicketAsync(Guid userId, CreateSupportTicketDto dto, CancellationToken cancellationToken = default);
    Task<SupportTicketDto?> GetTicketAsync(Guid ticketId, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<SupportTicketDto?> GetTicketByNumberAsync(string ticketNumber, Guid? userId = null, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
    Task<PagedResult<SupportTicketDto>> GetUserTicketsAsync(Guid userId, string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
    Task<PagedResult<SupportTicketDto>> GetAllTicketsAsync(string? status = null, string? category = null, Guid? assignedToId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> UpdateTicketAsync(Guid ticketId, UpdateSupportTicketDto dto, CancellationToken cancellationToken = default);
    Task<bool> AssignTicketAsync(Guid ticketId, Guid assignedToId, CancellationToken cancellationToken = default);
    Task<bool> CloseTicketAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<bool> ReopenTicketAsync(Guid ticketId, CancellationToken cancellationToken = default);

    // Messages
    Task<TicketMessageDto> AddMessageAsync(Guid userId, CreateTicketMessageDto dto, bool isStaffResponse = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<TicketMessageDto>> GetTicketMessagesAsync(Guid ticketId, bool includeInternal = false, CancellationToken cancellationToken = default);

    // Stats
    Task<TicketStatsDto> GetTicketStatsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<SupportTicketDto>> GetUnassignedTicketsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<SupportTicketDto>> GetMyAssignedTicketsAsync(Guid agentId, CancellationToken cancellationToken = default);
    
    // Agent Dashboard
    Task<SupportAgentDashboardDto> GetAgentDashboardAsync(Guid agentId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<List<CategoryTicketCountDto>> GetTicketsByCategoryAsync(Guid? agentId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<List<PriorityTicketCountDto>> GetTicketsByPriorityAsync(Guid? agentId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<List<TicketTrendDto>> GetTicketTrendsAsync(Guid? agentId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}
