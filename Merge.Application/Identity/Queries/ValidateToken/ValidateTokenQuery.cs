using MediatR;

namespace Merge.Application.Identity.Queries.ValidateToken;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ValidateTokenQuery(
    string Token) : IRequest<bool>;

