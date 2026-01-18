using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Commands.CreateStore;

public record CreateStoreCommand(
    Guid SellerId,
    CreateStoreDto Dto
) : IRequest<StoreDto>;
