using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetStore;

public record GetStoreQuery(
    Guid StoreId
) : IRequest<StoreDto?>;
