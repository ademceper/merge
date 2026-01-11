using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetUserSellerApplication;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetUserSellerApplicationQuery(
    Guid UserId
) : IRequest<SellerApplicationDto?>;
