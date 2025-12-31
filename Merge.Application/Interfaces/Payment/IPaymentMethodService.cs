using Merge.Application.DTOs.Payment;

namespace Merge.Application.Interfaces.Payment;

public interface IPaymentMethodService
{
    Task<PaymentMethodDto> CreatePaymentMethodAsync(CreatePaymentMethodDto dto);
    Task<PaymentMethodDto?> GetPaymentMethodByIdAsync(Guid id);
    Task<PaymentMethodDto?> GetPaymentMethodByCodeAsync(string code);
    Task<IEnumerable<PaymentMethodDto>> GetAllPaymentMethodsAsync(bool? isActive = null);
    Task<IEnumerable<PaymentMethodDto>> GetAvailablePaymentMethodsAsync(decimal orderAmount);
    Task<bool> UpdatePaymentMethodAsync(Guid id, UpdatePaymentMethodDto dto);
    Task<bool> DeletePaymentMethodAsync(Guid id);
    Task<bool> SetDefaultPaymentMethodAsync(Guid id);
    Task<decimal> CalculateProcessingFeeAsync(Guid paymentMethodId, decimal amount);
}

