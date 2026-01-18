using MediatR;

namespace Merge.Application.Seller.Commands.VerifyStore;

public record VerifyStoreCommand(
    Guid StoreId
) : IRequest<bool>;
