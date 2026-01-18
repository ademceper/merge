using MediatR;

namespace Merge.Application.Marketing.Commands.ApplyReferralCode;

public record ApplyReferralCodeCommand(
    Guid NewUserId,
    string Code) : IRequest<bool>;
