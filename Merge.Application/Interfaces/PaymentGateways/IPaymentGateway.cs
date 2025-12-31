using Merge.Application.DTOs;

namespace Merge.Application.Interfaces.PaymentGateways;

public interface IPaymentGateway
{
    string GatewayName { get; }
    Task<PaymentGatewayResponseDto> ProcessPaymentAsync(PaymentGatewayRequestDto request);
    Task<PaymentGatewayResponseDto> RefundPaymentAsync(string transactionId, decimal amount, string? reason = null);
    Task<PaymentGatewayStatusDto> GetPaymentStatusAsync(string transactionId);
    Task<bool> VerifyWebhookAsync(string signature, string payload);
    Task<PaymentGatewayWebhookDto?> ProcessWebhookAsync(string payload);
}

public class PaymentGatewayRequestDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public string OrderId { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public PaymentCardDto? Card { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
    public string? CallbackUrl { get; set; }
    public string? ReturnUrl { get; set; }
}

public class PaymentCardDto
{
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public string ExpiryMonth { get; set; } = string.Empty;
    public string ExpiryYear { get; set; } = string.Empty;
    public string Cvv { get; set; } = string.Empty;
}

public class PaymentGatewayResponseDto
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string? PaymentUrl { get; set; } // For 3D Secure redirects
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class PaymentGatewayStatusDto
{
    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Pending, Success, Failed, Refunded
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public DateTime? ProcessedAt { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class PaymentGatewayWebhookDto
{
    public string TransactionId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty; // payment.success, payment.failed, refund.success
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

