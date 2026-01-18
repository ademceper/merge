using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetSellerApplication;

public record GetSellerApplicationQuery(
    Guid ApplicationId
) : IRequest<SellerApplicationDto?>;
