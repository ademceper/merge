using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetStore;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetStoreQuery(
    Guid StoreId
) : IRequest<StoreDto?>;
