using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.DTOs.User;

public class UpdateUserPreferenceDto
{
    // Display Preferences
    [StringLength(50)]
    public string? Theme { get; set; }
    
    [StringLength(10)]
    public string? DefaultLanguage { get; set; }
    
    [StringLength(10)]
    public string? DefaultCurrency { get; set; }
    
    [Range(1, 100, ErrorMessage = "Sayfa başına öğe sayısı 1 ile 100 arasında olmalıdır.")]
    public int? ItemsPerPage { get; set; }
    
    [StringLength(50)]
    public string? DateFormat { get; set; }
    
    [StringLength(50)]
    public string? TimeFormat { get; set; }

    // Notification Preferences
    public bool? EmailNotifications { get; set; }
    
    public bool? SmsNotifications { get; set; }
    
    public bool? PushNotifications { get; set; }
    
    public bool? OrderUpdates { get; set; }
    
    public bool? PromotionalEmails { get; set; }
    
    public bool? ProductRecommendations { get; set; }
    
    public bool? ReviewReminders { get; set; }
    
    public bool? WishlistPriceAlerts { get; set; }
    
    public bool? NewsletterSubscription { get; set; }

    // Privacy Preferences
    public bool? ShowProfilePublicly { get; set; }
    
    public bool? ShowPurchaseHistory { get; set; }
    
    public bool? AllowPersonalization { get; set; }
    
    public bool? AllowDataCollection { get; set; }
    
    public bool? AllowThirdPartySharing { get; set; }

    // Shopping Preferences
    [StringLength(100)]
    public string? DefaultShippingAddress { get; set; }
    
    [StringLength(100)]
    public string? DefaultPaymentMethod { get; set; }
    
    public bool? AutoApplyCoupons { get; set; }
    
    public bool? SaveCartOnLogout { get; set; }
    
    public bool? ShowOutOfStockItems { get; set; }
}
