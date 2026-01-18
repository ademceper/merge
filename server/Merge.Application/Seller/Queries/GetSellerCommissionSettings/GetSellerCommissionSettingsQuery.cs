using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetSellerCommissionSettings;

public record GetSellerCommissionSettingsQuery(
    Guid SellerId
) : IRequest<SellerCommissionSettingsDto?>;
