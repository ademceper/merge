using MediatR;

namespace Merge.Application.Seller.Commands.SetPrimaryStore;

public record SetPrimaryStoreCommand(
    Guid SellerId,
    Guid StoreId
) : IRequest<bool>;
