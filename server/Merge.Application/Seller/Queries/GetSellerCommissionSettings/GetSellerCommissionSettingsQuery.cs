using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetSellerCommissionSettings;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetSellerCommissionSettingsQuery(
    Guid SellerId
) : IRequest<SellerCommissionSettingsDto?>;
