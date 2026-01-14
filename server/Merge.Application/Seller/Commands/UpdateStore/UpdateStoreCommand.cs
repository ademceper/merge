using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Commands.UpdateStore;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateStoreCommand(
    Guid StoreId,
    UpdateStoreDto Dto
) : IRequest<bool>;
