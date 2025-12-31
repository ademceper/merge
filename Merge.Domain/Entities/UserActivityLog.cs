namespace Merge.Domain.Entities;

public class UserActivityLog : BaseEntity
{
    public Guid? UserId { get; set; } // Nullable for anonymous users
    public string ActivityType { get; set; } = string.Empty; // Login, Logout, ViewProduct, AddToCart, Purchase, etc.
    public string EntityType { get; set; } = string.Empty; // Product, Order, Cart, etc.
    public Guid? EntityId { get; set; } // ID of the entity being acted upon
    public string Description { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty; // Mobile, Desktop, Tablet
    public string Browser { get; set; } = string.Empty;
    public string OS { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty; // City, Country
    public string Metadata { get; set; } = string.Empty; // JSON data for additional context
    public int DurationMs { get; set; } // Duration of action in milliseconds
    public bool WasSuccessful { get; set; } = true;
    public string? ErrorMessage { get; set; }

    // Navigation properties
    public User? User { get; set; }
}

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
