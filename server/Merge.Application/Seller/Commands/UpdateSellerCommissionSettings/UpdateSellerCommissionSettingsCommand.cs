using MediatR;
using Merge.Application.DTOs.Seller;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.Seller.Commands.UpdateSellerCommissionSettings;

public record UpdateSellerCommissionSettingsCommand(
    Guid SellerId,
    decimal? CustomCommissionRate,
    bool? UseCustomRate,
    decimal? MinimumPayoutAmount,
    string? PaymentMethod,
    string? PaymentDetails
) : IRequest<SellerCommissionSettingsDto>;
