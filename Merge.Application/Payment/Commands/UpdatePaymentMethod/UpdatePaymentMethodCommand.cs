using MediatR;
using Merge.Application.DTOs.Payment;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.Payment.Commands.UpdatePaymentMethod;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdatePaymentMethodCommand(
    Guid PaymentMethodId,
    string? Name,
    string? Description,
    string? IconUrl,
    bool? IsActive,
    bool? RequiresOnlinePayment,
    bool? RequiresManualVerification,
    decimal? MinimumAmount,
    decimal? MaximumAmount,
    decimal? ProcessingFee,
    decimal? ProcessingFeePercentage,
    PaymentMethodSettingsDto? Settings,
    int? DisplayOrder,
    bool? IsDefault
) : IRequest<bool>;
