using MediatR;

namespace Merge.Application.Seller.Commands.DeleteStore;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteStoreCommand(
    Guid StoreId
) : IRequest<bool>;
