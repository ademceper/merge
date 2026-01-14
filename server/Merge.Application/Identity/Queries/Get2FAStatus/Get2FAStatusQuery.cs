using MediatR;
using Merge.Application.DTOs.Identity;

namespace Merge.Application.Identity.Queries.Get2FAStatus;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record Get2FAStatusQuery(
    Guid UserId) : IRequest<TwoFactorStatusDto?>;

