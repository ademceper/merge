using Merge.Application.DTOs.Security;
using Merge.Application.Common;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Security;

public interface IPaymentFraudPreventionService
{
    Task<PaymentFraudPreventionDto> CheckPaymentAsync(CreatePaymentFraudCheckDto dto, CancellationToken cancellationToken = default);
    Task<PaymentFraudPreventionDto?> GetCheckByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentFraudPreventionDto>> GetBlockedPaymentsAsync(CancellationToken cancellationToken = default);
    Task<bool> BlockPaymentAsync(Guid checkId, string reason, CancellationToken cancellationToken = default);
    Task<bool> UnblockPaymentAsync(Guid checkId, CancellationToken cancellationToken = default);
    Task<PagedResult<PaymentFraudPreventionDto>> GetAllChecksAsync(string? status = null, bool? isBlocked = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
}


