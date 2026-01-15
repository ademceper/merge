using MediatR;

namespace Merge.Application.Identity.Queries.IsEmailVerified;

public record IsEmailVerifiedQuery(
    Guid UserId) : IRequest<bool>;

