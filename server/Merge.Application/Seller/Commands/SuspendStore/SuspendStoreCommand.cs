using MediatR;

namespace Merge.Application.Seller.Commands.SuspendStore;

public record SuspendStoreCommand(
    Guid StoreId,
    string Reason
) : IRequest<bool>;
