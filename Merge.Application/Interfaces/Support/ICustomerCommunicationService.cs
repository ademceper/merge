using Merge.Application.DTOs.Support;

namespace Merge.Application.Interfaces.Support;

public interface ICustomerCommunicationService
{
    Task<CustomerCommunicationDto> CreateCommunicationAsync(CreateCustomerCommunicationDto dto, Guid? sentByUserId = null);
    Task<CustomerCommunicationDto?> GetCommunicationAsync(Guid id);
    Task<IEnumerable<CustomerCommunicationDto>> GetUserCommunicationsAsync(Guid userId, string? communicationType = null, string? channel = null, int page = 1, int pageSize = 20);
    Task<CommunicationHistoryDto> GetUserCommunicationHistoryAsync(Guid userId);
    Task<IEnumerable<CustomerCommunicationDto>> GetAllCommunicationsAsync(string? communicationType = null, string? channel = null, Guid? userId = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20);
    Task<bool> UpdateCommunicationStatusAsync(Guid id, string status, DateTime? deliveredAt = null, DateTime? readAt = null);
    Task<Dictionary<string, int>> GetCommunicationStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
}

