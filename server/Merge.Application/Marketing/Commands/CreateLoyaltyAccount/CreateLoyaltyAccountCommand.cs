using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.CreateLoyaltyAccount;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateLoyaltyAccountCommand(
    Guid UserId) : IRequest<LoyaltyAccountDto>;
