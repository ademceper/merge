using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UserEntity = Merge.Domain.Modules.Identity.User;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Domain.Entities;
using Merge.Application.DTOs.User;
using Microsoft.Extensions.Logging;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;


namespace Merge.Application.Services.User;

public class UserPreferenceService : IUserPreferenceService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UserPreferenceService> _logger;

    public UserPreferenceService(
        IDbContext context,
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
            preferences = UserPreference.Create(userId);

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
            preferences = UserPreference.Create(userId);
            await _context.Set<UserPreference>().AddAsync(preferences, cancellationToken);
        }

        // ✅ BOLUM 1.1: Domain Method use (Encapsulation)
        preferences.UpdatePreferences(
            theme: dto.Theme,
            defaultLanguage: dto.DefaultLanguage,
            defaultCurrency: dto.DefaultCurrency,
            itemsPerPage: dto.ItemsPerPage,
            dateFormat: dto.DateFormat,
            timeFormat: dto.TimeFormat,
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
            preferences = UserPreference.Create(userId);
            await _context.Set<UserPreference>().AddAsync(preferences, cancellationToken);
        }
        else
        {
            // Reset to defaults
            preferences.ResetToDefaults();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<UserPreferenceDto>(preferences);
    }

}
