using MediatR;
using Merge.Application.DTOs.Seller;
using Merge.Domain.Enums;

namespace Merge.Application.Seller.Queries.GetSellerCommissions;

public record GetSellerCommissionsQuery(
    Guid SellerId,
    CommissionStatus? Status = null
) : IRequest<IEnumerable<SellerCommissionDto>>;
