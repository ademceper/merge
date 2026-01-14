using MediatR;

namespace Merge.Application.Seller.Commands.SetPrimaryStore;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SetPrimaryStoreCommand(
    Guid SellerId,
    Guid StoreId
) : IRequest<bool>;
