using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.CreateReferralCode;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateReferralCodeCommand(
    Guid UserId) : IRequest<ReferralCodeDto>;
