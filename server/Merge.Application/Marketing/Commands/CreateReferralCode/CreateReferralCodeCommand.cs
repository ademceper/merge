using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.CreateReferralCode;

public record CreateReferralCodeCommand(
    Guid UserId) : IRequest<ReferralCodeDto>;
