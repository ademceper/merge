using MediatR;
using Merge.Application.DTOs.Seller;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.Seller.Commands.UpdateSellerCommissionSettings;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateSellerCommissionSettingsCommand(
    Guid SellerId,
    decimal? CustomCommissionRate,
    bool? UseCustomRate,
    decimal? MinimumPayoutAmount,
    string? PaymentMethod,
    string? PaymentDetails
) : IRequest<SellerCommissionSettingsDto>;
