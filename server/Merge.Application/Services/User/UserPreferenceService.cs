using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UserEntity = Merge.Domain.Modules.Identity.User;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Domain.Entities;
using Merge.Application.DTOs.User;
using Microsoft.Extensions.Logging;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Services.User;

public class UserPreferenceService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserPreferenceService> logger) : IUserPreferenceService
{

    public async Task<UserPreferenceDto> GetUserPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving preferences for user: {UserId}", userId);

        var preferences = await context.Set<UserPreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(up => up.UserId == userId, cancellationToken);

        if (preferences == null)
        {
            logger.LogInformation("No preferences found for user: {UserId}, creating default preferences", userId);

            // Create default preferences
            preferences = UserPreference.Create(userId);

            await context.Set<UserPreference>().AddAsync(preferences, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Default preferences created for user: {UserId}", userId);
        }

        return mapper.Map<UserPreferenceDto>(preferences);
    }

    public async Task<UserPreferenceDto> UpdateUserPreferencesAsync(Guid userId, UpdateUserPreferenceDto dto, CancellationToken cancellationToken = default)
    {
        var preferences = await context.Set<UserPreference>()
            .FirstOrDefaultAsync(up => up.UserId == userId, cancellationToken);

        if (preferences == null)
        {
            preferences = UserPreference.Create(userId);
            await context.Set<UserPreference>().AddAsync(preferences, cancellationToken);
        }

        // Parse enum values from strings
        Theme? theme = null;
        if (!string.IsNullOrEmpty(dto.Theme) && Enum.TryParse<Theme>(dto.Theme, true, out var parsedTheme))
            theme = parsedTheme;

        TimeFormat? timeFormat = null;
        if (!string.IsNullOrEmpty(dto.TimeFormat) && Enum.TryParse<TimeFormat>(dto.TimeFormat, true, out var parsedTimeFormat))
            timeFormat = parsedTimeFormat;

        preferences.UpdatePreferences(
            theme: theme,
            defaultLanguage: dto.DefaultLanguage,
            defaultCurrency: dto.DefaultCurrency,
            itemsPerPage: dto.ItemsPerPage,
            dateFormat: dto.DateFormat,
            timeFormat: timeFormat,
            emailNotifications: dto.EmailNotifications,
            smsNotifications: dto.SmsNotifications,
            pushNotifications: dto.PushNotifications,
            orderUpdates: dto.OrderUpdates,
            promotionalEmails: dto.PromotionalEmails,
            productRecommendations: dto.ProductRecommendations,
            reviewReminders: dto.ReviewReminders,
            wishlistPriceAlerts: dto.WishlistPriceAlerts,
            newsletterSubscription: dto.NewsletterSubscription,
            showProfilePublicly: dto.ShowProfilePublicly,
            showPurchaseHistory: dto.ShowPurchaseHistory,
            allowPersonalization: dto.AllowPersonalization,
            allowDataCollection: dto.AllowDataCollection,
            allowThirdPartySharing: dto.AllowThirdPartySharing,
            defaultShippingAddress: dto.DefaultShippingAddress,
            defaultPaymentMethod: dto.DefaultPaymentMethod,
            autoApplyCoupons: dto.AutoApplyCoupons,
            saveCartOnLogout: dto.SaveCartOnLogout,
            showOutOfStockItems: dto.ShowOutOfStockItems
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<UserPreferenceDto>(preferences);
    }

    public async Task<UserPreferenceDto> ResetToDefaultsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var preferences = await context.Set<UserPreference>()
            .FirstOrDefaultAsync(up => up.UserId == userId, cancellationToken);

        if (preferences == null)
        {
            preferences = UserPreference.Create(userId);
            await context.Set<UserPreference>().AddAsync(preferences, cancellationToken);
        }
        else
        {
            // Reset to defaults
            preferences.ResetToDefaults();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<UserPreferenceDto>(preferences);
    }

}
