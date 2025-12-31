using Merge.Application.DTOs.Payment;

namespace Merge.Application.Interfaces.Payment;

public interface IPaymentService
{
    Task<PaymentDto?> GetByIdAsync(Guid id);
    Task<PaymentDto?> GetByOrderIdAsync(Guid orderId);
    Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto dto);
    Task<PaymentDto> ProcessPaymentAsync(Guid paymentId, ProcessPaymentDto dto);
    Task<PaymentDto> RefundPaymentAsync(Guid paymentId, decimal? amount = null);
    Task<bool> VerifyPaymentAsync(string transactionId);
}

