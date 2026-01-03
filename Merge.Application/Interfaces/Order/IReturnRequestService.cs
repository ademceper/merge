using Merge.Application.DTOs.Order;

namespace Merge.Application.Interfaces.Order;

public interface IReturnRequestService
{
    Task<ReturnRequestDto?> GetByIdAsync(Guid id);
    // ✅ PERFORMANCE: Pagination eklendi - unbounded query önleme
    Task<IEnumerable<ReturnRequestDto>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReturnRequestDto>> GetAllAsync(string? status = null);
    Task<ReturnRequestDto> CreateAsync(CreateReturnRequestDto dto);
    Task<ReturnRequestDto> UpdateStatusAsync(Guid id, string status, string? rejectionReason = null);
    Task<bool> ApproveAsync(Guid id);
    Task<bool> RejectAsync(Guid id, string reason);
    Task<bool> CompleteAsync(Guid id, string trackingNumber);
}

