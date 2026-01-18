using Merge.Domain.Modules.Ordering;
namespace Merge.Application.DTOs.Cart;


public record AbandonedCartDto(
    Guid CartId,
    Guid UserId,
    string UserEmail,
    string UserName,
    int ItemCount,
    decimal TotalValue,
    DateTime LastModified,
    int HoursSinceAbandonment,
    IReadOnlyList<CartItemDto> Items,
    bool HasReceivedEmail,
    int EmailsSentCount,
    DateTime? LastEmailSent
);
