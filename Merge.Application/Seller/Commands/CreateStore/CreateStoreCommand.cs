using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Commands.CreateStore;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateStoreCommand(
    Guid SellerId,
    CreateStoreDto Dto
) : IRequest<StoreDto>;
