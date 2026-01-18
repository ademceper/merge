using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetUserSellerApplication;

public record GetUserSellerApplicationQuery(
    Guid UserId
) : IRequest<SellerApplicationDto?>;
