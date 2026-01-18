using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.PurchaseGiftCard;

public record PurchaseGiftCardCommand(
    Guid UserId,
    decimal Amount,
    Guid? AssignedToUserId,
    string? Message,
    DateTime? ExpiresAt) : IRequest<GiftCardDto>;
