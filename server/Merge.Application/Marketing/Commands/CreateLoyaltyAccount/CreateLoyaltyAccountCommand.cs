using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.CreateLoyaltyAccount;

public record CreateLoyaltyAccountCommand(
    Guid UserId) : IRequest<LoyaltyAccountDto>;
