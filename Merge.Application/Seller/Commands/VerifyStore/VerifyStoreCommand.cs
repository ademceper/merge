using MediatR;

namespace Merge.Application.Seller.Commands.VerifyStore;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record VerifyStoreCommand(
    Guid StoreId
) : IRequest<bool>;
