using Merge.Application.DTOs.Payment;
using Merge.Domain.Modules.Payment;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Payment;

public interface IPaymentMethodService
{
    Task<PaymentMethodDto> CreatePaymentMethodAsync(CreatePaymentMethodDto dto, CancellationToken cancellationToken = default);
    Task<PaymentMethodDto?> GetPaymentMethodByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaymentMethodDto?> GetPaymentMethodByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentMethodDto>> GetAllPaymentMethodsAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentMethodDto>> GetAvailablePaymentMethodsAsync(decimal orderAmount, CancellationToken cancellationToken = default);
    Task<bool> UpdatePaymentMethodAsync(Guid id, UpdatePaymentMethodDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeletePaymentMethodAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SetDefaultPaymentMethodAsync(Guid id, CancellationToken cancellationToken = default);
    Task<decimal> CalculateProcessingFeeAsync(Guid paymentMethodId, decimal amount, CancellationToken cancellationToken = default);
}

