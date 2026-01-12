using Merge.Application.DTOs.Payment;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.Interfaces.Payment;

public interface IPaymentService
{
    Task<PaymentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaymentDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto dto, CancellationToken cancellationToken = default);
    Task<PaymentDto> ProcessPaymentAsync(Guid paymentId, ProcessPaymentDto dto, CancellationToken cancellationToken = default);
    Task<PaymentDto> RefundPaymentAsync(Guid paymentId, decimal? amount = null, CancellationToken cancellationToken = default);
    Task<bool> VerifyPaymentAsync(string transactionId, CancellationToken cancellationToken = default);
}

