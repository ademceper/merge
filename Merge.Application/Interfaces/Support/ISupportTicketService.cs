using Merge.Application.DTOs.Support;

namespace Merge.Application.Interfaces.Support;

public interface ISupportTicketService
{
    // Ticket management
    Task<SupportTicketDto> CreateTicketAsync(Guid userId, CreateSupportTicketDto dto);
    Task<SupportTicketDto?> GetTicketAsync(Guid ticketId, Guid? userId = null);
    Task<SupportTicketDto?> GetTicketByNumberAsync(string ticketNumber, Guid? userId = null);
    Task<IEnumerable<SupportTicketDto>> GetUserTicketsAsync(Guid userId, string? status = null);
    Task<IEnumerable<SupportTicketDto>> GetAllTicketsAsync(string? status = null, string? category = null, Guid? assignedToId = null, int page = 1, int pageSize = 20);
    Task<bool> UpdateTicketAsync(Guid ticketId, UpdateSupportTicketDto dto);
    Task<bool> AssignTicketAsync(Guid ticketId, Guid assignedToId);
    Task<bool> CloseTicketAsync(Guid ticketId);
    Task<bool> ReopenTicketAsync(Guid ticketId);

    // Messages
    Task<TicketMessageDto> AddMessageAsync(Guid userId, CreateTicketMessageDto dto, bool isStaffResponse = false);
    Task<IEnumerable<TicketMessageDto>> GetTicketMessagesAsync(Guid ticketId, bool includeInternal = false);

    // Stats
    Task<TicketStatsDto> GetTicketStatsAsync();
    Task<IEnumerable<SupportTicketDto>> GetUnassignedTicketsAsync();
    Task<IEnumerable<SupportTicketDto>> GetMyAssignedTicketsAsync(Guid agentId);
    
    // Agent Dashboard
    Task<SupportAgentDashboardDto> GetAgentDashboardAsync(Guid agentId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<CategoryTicketCountDto>> GetTicketsByCategoryAsync(Guid? agentId = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<PriorityTicketCountDto>> GetTicketsByPriorityAsync(Guid? agentId = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<TicketTrendDto>> GetTicketTrendsAsync(Guid? agentId = null, DateTime? startDate = null, DateTime? endDate = null);
}
