using MediatR;

namespace Merge.Application.Seller.Commands.DeleteStore;

public record DeleteStoreCommand(
    Guid StoreId
) : IRequest<bool>;
