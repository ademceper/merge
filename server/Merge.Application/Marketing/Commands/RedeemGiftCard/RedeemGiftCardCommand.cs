using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.RedeemGiftCard;

public record RedeemGiftCardCommand(
    string Code,
    Guid UserId) : IRequest<GiftCardDto>;
