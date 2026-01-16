using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Configuration;
using Merge.Application.Services.Notification;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using UserEntity = Merge.Domain.Modules.Identity.User;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Marketplace.SellerApplication>;

namespace Merge.Application.Seller.Commands.ApproveSellerApplication;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ApproveSellerApplicationCommandHandler : IRequestHandler<ApproveSellerApplicationCommand, bool>
{
    private readonly IRepository _applicationRepository;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IOptions<SellerSettings> _sellerSettings;
    private readonly ILogger<ApproveSellerApplicationCommandHandler> _logger;

    public ApproveSellerApplicationCommandHandler(
        IRepository applicationRepository,
        UserManager<UserEntity> userManager,
        IDbContext context,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IOptions<SellerSettings> sellerSettings,
        ILogger<ApproveSellerApplicationCommandHandler> logger)
    {
        _applicationRepository = applicationRepository;
        _userManager = userManager;
        _context = context;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _sellerSettings = sellerSettings;
        _logger = logger;
    }

    public async Task<bool> Handle(ApproveSellerApplicationCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Approving seller application {ApplicationId} by reviewer {ReviewerId}",
            request.ApplicationId, request.ReviewerId);

        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var application = await _applicationRepository.GetByIdAsync(request.ApplicationId);
            if (application == null)
            {
                _logger.LogWarning("Application approval failed - Application {ApplicationId} not found", request.ApplicationId);
                return false;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            application.Approve(request.ReviewerId);

            await _applicationRepository.UpdateAsync(application);
            await CreateSellerProfileAsync(application, cancellationToken);

            // Send approval email
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == application.UserId, cancellationToken);
            if (user != null)
            {
                await _emailService.SendEmailAsync(
                    user.Email ?? string.Empty,
                    "Seller Application Approved!",
                    $"Congratulations! Your seller application for {application.BusinessName} has been approved. " +
                    "You can now start selling on our platform.",
                    true,
                    cancellationToken
                );

                // Update user role to Seller
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }
                await _userManager.AddToRoleAsync(user, "Seller");
            }

            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Application {ApplicationId} approved successfully", request.ApplicationId);
            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error approving application {ApplicationId}", request.ApplicationId);
            throw;
        }
    }

    private async Task CreateSellerProfileAsync(SellerApplication application, CancellationToken cancellationToken)
    {
        // Check if seller profile already exists
        var existingProfile = await _context.Set<SellerProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == application.UserId, cancellationToken);

        if (existingProfile != null)
        {
            _logger.LogInformation("Seller profile already exists for user {UserId}", application.UserId);
            return;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        // ✅ BOLUM 12.0: Magic number config'den - SellerSettings kullanımı
        var profile = SellerProfile.Create(
            userId: application.UserId,
            storeName: application.BusinessName,
            commissionRate: _sellerSettings.Value.DefaultCommissionRate);

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        profile.Verify();
        profile.Activate();

        await _context.Set<SellerProfile>().AddAsync(profile, cancellationToken);
        _logger.LogInformation("Created seller profile for user {UserId} with store name: {StoreName}",
            application.UserId, application.BusinessName);
    }
}
