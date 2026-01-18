using Merge.Application.DTOs.Security;
using Merge.Application.Common;

namespace Merge.Application.Interfaces.Security;

public interface IOrderVerificationService
{
    Task<OrderVerificationDto> CreateVerificationAsync(CreateOrderVerificationDto dto, CancellationToken cancellationToken = default);
    Task<OrderVerificationDto?> GetVerificationByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderVerificationDto>> GetPendingVerificationsAsync(CancellationToken cancellationToken = default);
    Task<bool> VerifyOrderAsync(Guid verificationId, Guid verifiedByUserId, string? notes = null, CancellationToken cancellationToken = default);
    Task<bool> RejectOrderAsync(Guid verificationId, Guid verifiedByUserId, string reason, CancellationToken cancellationToken = default);
    Task<PagedResult<OrderVerificationDto>> GetAllVerificationsAsync(string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
}


