using MediatR;

namespace Merge.Application.Identity.Queries.IsEmailVerified;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record IsEmailVerifiedQuery(
    Guid UserId) : IRequest<bool>;

