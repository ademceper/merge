using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetCommission;

public record GetCommissionQuery(
    Guid CommissionId
) : IRequest<SellerCommissionDto?>;
