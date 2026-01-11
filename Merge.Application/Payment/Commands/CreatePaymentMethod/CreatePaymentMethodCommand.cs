using MediatR;
using Merge.Application.DTOs.Payment;

namespace Merge.Application.Payment.Commands.CreatePaymentMethod;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreatePaymentMethodCommand(
    string Name,
    string Code,
    string Description,
    string IconUrl,
    bool IsActive,
    bool RequiresOnlinePayment,
    bool RequiresManualVerification,
    decimal? MinimumAmount,
    decimal? MaximumAmount,
    decimal? ProcessingFee,
    decimal? ProcessingFeePercentage,
    PaymentMethodSettingsDto? Settings,
    int DisplayOrder,
    bool IsDefault
) : IRequest<PaymentMethodDto>;
