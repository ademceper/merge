using Merge.Application.DTOs.Security;
namespace Merge.Application.Interfaces.Security;

public interface IPaymentFraudPreventionService
{
    Task<PaymentFraudPreventionDto> CheckPaymentAsync(CreatePaymentFraudCheckDto dto);
    Task<PaymentFraudPreventionDto?> GetCheckByPaymentIdAsync(Guid paymentId);
    Task<IEnumerable<PaymentFraudPreventionDto>> GetBlockedPaymentsAsync();
    Task<bool> BlockPaymentAsync(Guid checkId, string reason);
    Task<bool> UnblockPaymentAsync(Guid checkId);
    Task<IEnumerable<PaymentFraudPreventionDto>> GetAllChecksAsync(string? status = null, bool? isBlocked = null, int page = 1, int pageSize = 20);
}


