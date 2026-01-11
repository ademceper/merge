using MediatR;

namespace Merge.Application.Seller.Commands.SuspendStore;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SuspendStoreCommand(
    Guid StoreId,
    string Reason
) : IRequest<bool>;
