using MediatR;

namespace Merge.Application.Identity.Queries.ValidateToken;

public record ValidateTokenQuery(
    string Token) : IRequest<bool>;

