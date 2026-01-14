using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.RedeemGiftCard;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RedeemGiftCardCommand(
    string Code,
    Guid UserId) : IRequest<GiftCardDto>;
