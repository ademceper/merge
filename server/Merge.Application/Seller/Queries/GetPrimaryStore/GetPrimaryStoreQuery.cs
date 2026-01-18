using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetPrimaryStore;

public record GetPrimaryStoreQuery(
    Guid SellerId
) : IRequest<StoreDto?>;
