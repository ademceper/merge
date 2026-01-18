using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Commands.UpdateStore;

public record UpdateStoreCommand(
    Guid StoreId,
    UpdateStoreDto Dto
) : IRequest<bool>;
