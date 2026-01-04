using Merge.Application.DTOs.Order;
using Merge.Application.Common;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Order;

public interface IReturnRequestService
{
    Task<ReturnRequestDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    // ✅ PERFORMANCE: Pagination eklendi - unbounded query önleme
    Task<PagedResult<ReturnRequestDto>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<PagedResult<ReturnRequestDto>> GetAllAsync(string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ReturnRequestDto> CreateAsync(CreateReturnRequestDto dto, CancellationToken cancellationToken = default);
    Task<ReturnRequestDto> UpdateStatusAsync(Guid id, string status, string? rejectionReason = null, CancellationToken cancellationToken = default);
    Task<bool> ApproveAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> RejectAsync(Guid id, string reason, CancellationToken cancellationToken = default);
    Task<bool> CompleteAsync(Guid id, string trackingNumber, CancellationToken cancellationToken = default);
}

