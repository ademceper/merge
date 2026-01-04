using Merge.Application.DTOs.Support;
using Merge.Application.Common;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
namespace Merge.Application.Interfaces.Support;

public interface ICustomerCommunicationService
{
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    Task<CustomerCommunicationDto> CreateCommunicationAsync(CreateCustomerCommunicationDto dto, Guid? sentByUserId = null, CancellationToken cancellationToken = default);
    Task<CustomerCommunicationDto?> GetCommunicationAsync(Guid id, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
    Task<PagedResult<CustomerCommunicationDto>> GetUserCommunicationsAsync(Guid userId, string? communicationType = null, string? channel = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<CommunicationHistoryDto> GetUserCommunicationHistoryAsync(Guid userId, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
    Task<PagedResult<CustomerCommunicationDto>> GetAllCommunicationsAsync(string? communicationType = null, string? channel = null, Guid? userId = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> UpdateCommunicationStatusAsync(Guid id, string status, DateTime? deliveredAt = null, DateTime? readAt = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetCommunicationStatsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}

