using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetLoyaltyAccount;

public record GetLoyaltyAccountQuery(
    Guid UserId) : IRequest<LoyaltyAccountDto?>;
