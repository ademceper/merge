using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetSellerApplication;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetSellerApplicationQuery(
    Guid ApplicationId
) : IRequest<SellerApplicationDto?>;
