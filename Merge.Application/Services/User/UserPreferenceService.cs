using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UserEntity = Merge.Domain.Entities.User;
using Merge.Application.Interfaces.User;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Application.DTOs.User;
using Merge.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;


namespace Merge.Application.Services.User;

public class UserPreferenceService : IUserPreferenceService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UserPreferenceService> _logger;

    public UserPreferenceService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UserPreferenceService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<UserPreferenceDto> GetUserPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving preferences for user: {UserId}", userId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !up.IsDeleted (Global Query Filter)
        var preferences = await _context.Set<UserPreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(up => up.UserId == userId, cancellationToken);

        if (preferences == null)
        {
            _logger.LogInformation("No preferences found for user: {UserId}, creating default preferences", userId);

            // Create default preferences
            preferences = new UserPreference
            {
                UserId = userId
            };

            await _context.Set<UserPreference>().AddAsync(preferences, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Default preferences created for user: {UserId}", userId);
        }

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<UserPreferenceDto>(preferences);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<UserPreferenceDto> UpdateUserPreferencesAsync(Guid userId, UpdateUserPreferenceDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var preferences = await _context.Set<UserPreference>()
            .FirstOrDefaultAsync(up => up.UserId == userId, cancellationToken);

        if (preferences == null)
        {
            preferences = new UserPreference
            {
                UserId = userId
            };
            await _context.Set<UserPreference>().AddAsync(preferences, cancellationToken);
        }

        // Display Preferences
        if (dto.Theme != null) preferences.Theme = dto.Theme;
        if (dto.DefaultLanguage != null) preferences.DefaultLanguage = dto.DefaultLanguage;
        if (dto.DefaultCurrency != null) preferences.DefaultCurrency = dto.DefaultCurrency;
        if (dto.ItemsPerPage.HasValue) preferences.ItemsPerPage = dto.ItemsPerPage.Value;
        if (dto.DateFormat != null) preferences.DateFormat = dto.DateFormat;
        if (dto.TimeFormat != null) preferences.TimeFormat = dto.TimeFormat;

        // Notification Preferences
        if (dto.EmailNotifications.HasValue) preferences.EmailNotifications = dto.EmailNotifications.Value;
        if (dto.SmsNotifications.HasValue) preferences.SmsNotifications = dto.SmsNotifications.Value;
        if (dto.PushNotifications.HasValue) preferences.PushNotifications = dto.PushNotifications.Value;
        if (dto.OrderUpdates.HasValue) preferences.OrderUpdates = dto.OrderUpdates.Value;
        if (dto.PromotionalEmails.HasValue) preferences.PromotionalEmails = dto.PromotionalEmails.Value;
        if (dto.ProductRecommendations.HasValue) preferences.ProductRecommendations = dto.ProductRecommendations.Value;
        if (dto.ReviewReminders.HasValue) preferences.ReviewReminders = dto.ReviewReminders.Value;
        if (dto.WishlistPriceAlerts.HasValue) preferences.WishlistPriceAlerts = dto.WishlistPriceAlerts.Value;
        if (dto.NewsletterSubscription.HasValue) preferences.NewsletterSubscription = dto.NewsletterSubscription.Value;

        // Privacy Preferences
        if (dto.ShowProfilePublicly.HasValue) preferences.ShowProfilePublicly = dto.ShowProfilePublicly.Value;
        if (dto.ShowPurchaseHistory.HasValue) preferences.ShowPurchaseHistory = dto.ShowPurchaseHistory.Value;
        if (dto.AllowPersonalization.HasValue) preferences.AllowPersonalization = dto.AllowPersonalization.Value;
        if (dto.AllowDataCollection.HasValue) preferences.AllowDataCollection = dto.AllowDataCollection.Value;
        if (dto.AllowThirdPartySharing.HasValue) preferences.AllowThirdPartySharing = dto.AllowThirdPartySharing.Value;

        // Shopping Preferences
        if (dto.DefaultShippingAddress != null) preferences.DefaultShippingAddress = dto.DefaultShippingAddress;
        if (dto.DefaultPaymentMethod != null) preferences.DefaultPaymentMethod = dto.DefaultPaymentMethod;
        if (dto.AutoApplyCoupons.HasValue) preferences.AutoApplyCoupons = dto.AutoApplyCoupons.Value;
        if (dto.SaveCartOnLogout.HasValue) preferences.SaveCartOnLogout = dto.SaveCartOnLogout.Value;
        if (dto.ShowOutOfStockItems.HasValue) preferences.ShowOutOfStockItems = dto.ShowOutOfStockItems.Value;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<UserPreferenceDto>(preferences);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<UserPreferenceDto> ResetToDefaultsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var preferences = await _context.Set<UserPreference>()
            .FirstOrDefaultAsync(up => up.UserId == userId, cancellationToken);

        if (preferences == null)
        {
            preferences = new UserPreference
            {
                UserId = userId
            };
            await _context.Set<UserPreference>().AddAsync(preferences, cancellationToken);
        }
        else
        {
            // Reset to defaults
            preferences.Theme = "Light";
            preferences.DefaultLanguage = "tr-TR";
            preferences.DefaultCurrency = "TRY";
            preferences.ItemsPerPage = 20;
            preferences.DateFormat = "dd/MM/yyyy";
            preferences.TimeFormat = "24h";

            preferences.EmailNotifications = true;
            preferences.SmsNotifications = false;
            preferences.PushNotifications = true;
            preferences.OrderUpdates = true;
            preferences.PromotionalEmails = true;
            preferences.ProductRecommendations = true;
            preferences.ReviewReminders = true;
            preferences.WishlistPriceAlerts = true;
            preferences.NewsletterSubscription = false;

            preferences.ShowProfilePublicly = false;
            preferences.ShowPurchaseHistory = false;
            preferences.AllowPersonalization = true;
            preferences.AllowDataCollection = true;
            preferences.AllowThirdPartySharing = false;

            preferences.DefaultShippingAddress = string.Empty;
            preferences.DefaultPaymentMethod = string.Empty;
            preferences.AutoApplyCoupons = true;
            preferences.SaveCartOnLogout = true;
            preferences.ShowOutOfStockItems = false;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<UserPreferenceDto>(preferences);
    }

}
