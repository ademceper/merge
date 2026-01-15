using MediatR;
using Merge.Application.DTOs.Identity;

namespace Merge.Application.Identity.Queries.Get2FAStatus;

public record Get2FAStatusQuery(
    Guid UserId) : IRequest<TwoFactorStatusDto?>;

