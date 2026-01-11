using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetPrimaryStore;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetPrimaryStoreQuery(
    Guid SellerId
) : IRequest<StoreDto?>;
