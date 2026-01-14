using MediatR;

namespace Merge.Application.Marketing.Commands.ApplyReferralCode;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ApplyReferralCodeCommand(
    Guid NewUserId,
    string Code) : IRequest<bool>;
