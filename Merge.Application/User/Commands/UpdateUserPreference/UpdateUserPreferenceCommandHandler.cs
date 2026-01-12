using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.User;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.User.Commands.UpdateUserPreference;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class UpdateUserPreferenceCommandHandler : IRequestHandler<UpdateUserPreferenceCommand, UserPreferenceDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateUserPreferenceCommandHandler> _logger;

    public UpdateUserPreferenceCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateUserPreferenceCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserPreferenceDto> Handle(UpdateUserPreferenceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating preferences for user: {UserId}", request.UserId);

        var preferences = await _context.Set<UserPreference>()
            .FirstOrDefaultAsync(up => up.UserId == request.UserId, cancellationToken);

        if (preferences == null)
        {
            _logger.LogInformation("No preferences found for user: {UserId}, creating default preferences", request.UserId);
            preferences = UserPreference.Create(request.UserId);
            await _context.Set<UserPreference>().AddAsync(preferences, cancellationToken);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        preferences.UpdatePreferences(
            theme: request.Theme,
            defaultLanguage: request.DefaultLanguage,
            defaultCurrency: request.DefaultCurrency,
            itemsPerPage: request.ItemsPerPage,
            dateFormat: request.DateFormat,
            timeFormat: request.TimeFormat,
            emailNotifications: request.EmailNotifications,
            smsNotifications: request.SmsNotifications,
            pushNotifications: request.PushNotifications,
            orderUpdates: request.OrderUpdates,
            promotionalEmails: request.PromotionalEmails,
            productRecommendations: request.ProductRecommendations,
            reviewReminders: request.ReviewReminders,
            wishlistPriceAlerts: request.WishlistPriceAlerts,
            newsletterSubscription: request.NewsletterSubscription,
            showProfilePublicly: request.ShowProfilePublicly,
            showPurchaseHistory: request.ShowPurchaseHistory,
            allowPersonalization: request.AllowPersonalization,
            allowDataCollection: request.AllowDataCollection,
            allowThirdPartySharing: request.AllowThirdPartySharing,
            defaultShippingAddress: request.DefaultShippingAddress,
            defaultPaymentMethod: request.DefaultPaymentMethod,
            autoApplyCoupons: request.AutoApplyCoupons,
            saveCartOnLogout: request.SaveCartOnLogout,
            showOutOfStockItems: request.ShowOutOfStockItems);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Preferences updated successfully for user: {UserId}", request.UserId);

        return _mapper.Map<UserPreferenceDto>(preferences);
    }
}
