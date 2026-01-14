using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
namespace Merge.Domain.Enums;

/// <summary>
/// Activity Type - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum ActivityType
{
    // Authentication
    Login,
    Logout,
    Register,
    PasswordReset,
    TwoFactorEnabled,

    // Product Activities
    ViewProduct,
    SearchProduct,
    FilterProducts,

    // Cart Activities
    AddToCart,
    RemoveFromCart,
    UpdateCartQuantity,
    ClearCart,

    // Wishlist Activities
    AddToWishlist,
    RemoveFromWishlist,

    // Order Activities
    CreateOrder,
    CancelOrder,
    UpdateOrder,
    TrackOrder,

    // Payment Activities
    InitiatePayment,
    CompletePayment,
    FailedPayment,
    RefundInitiated,

    // Review Activities
    WriteReview,
    UpdateReview,
    DeleteReview,

    // Account Activities
    UpdateProfile,
    ChangePassword,
    AddAddress,
    UpdateAddress,
    DeleteAddress,

    // Admin Activities
    CreateProduct,
    UpdateProduct,
    DeleteProduct,
    CreateCoupon,
    UpdateCoupon,
    DeleteCoupon,

    // Other
    ContactSupport,
    DownloadInvoice,
    ApplyCoupon,
    ShareProduct
}

