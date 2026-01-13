namespace Merge.Domain.Enums;

/// <summary>
/// Entity Type Enum - BOLUM 1.2: Enum kullanımı (string EntityType YASAK)
/// </summary>
public enum EntityType
{
    User = 0,
    Product = 1,
    Order = 2,
    Cart = 3,
    Payment = 4,
    Review = 5,
    Address = 6,
    Coupon = 7,
    Category = 8,
    Brand = 9,
    Wishlist = 10,
    Invoice = 11,
    Shipment = 12,
    ReturnRequest = 13,
    Notification = 14,
    Organization = 15,
    Team = 16,
    TeamMember = 17,
    B2BUser = 18,
    SecurityAlert = 19,
    AccountSecurityEvent = 20,
    Other = 99
}
