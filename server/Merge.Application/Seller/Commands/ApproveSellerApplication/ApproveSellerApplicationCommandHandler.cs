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

public class ApproveSellerApplicationCommandHandler(IRepository applicationRepository, UserManager<UserEntity> userManager, IDbContext context, IUnitOfWork unitOfWork, IEmailService emailService, IOptions<SellerSettings> sellerSettings, ILogger<ApproveSellerApplicationCommandHandler> logger) : IRequestHandler<ApproveSellerApplicationCommand, bool>
{
    private readonly SellerSettings sellerConfig = sellerSettings.Value;

    public async Task<bool> Handle(ApproveSellerApplicationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Approving seller application {ApplicationId} by reviewer {ReviewerId}",
            request.ApplicationId, request.ReviewerId);

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            var application = await applicationRepository.GetByIdAsync(request.ApplicationId);
            if (application is null)
            {
                logger.LogWarning("Application approval failed - Application {ApplicationId} not found", request.ApplicationId);
                return false;
            }

            application.Approve(request.ReviewerId);

            await applicationRepository.UpdateAsync(application);
            await CreateSellerProfileAsync(application, cancellationToken);

            // Send approval email
            var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == application.UserId, cancellationToken);
            if (user is not null)
            {
                await emailService.SendEmailAsync(
                    user.Email ?? string.Empty,
                    "Seller Application Approved!",
                    $"Congratulations! Your seller application for {application.BusinessName} has been approved. " +
                    "You can now start selling on our platform.",
                    true,
                    cancellationToken
                );

                // Add Seller role (keep existing roles like Customer)
                var currentRoles = await userManager.GetRolesAsync(user);
                if (!currentRoles.Contains("Seller"))
                {
                    await userManager.AddToRoleAsync(user, "Seller");
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Application {ApplicationId} approved successfully", request.ApplicationId);
            return true;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Error approving application {ApplicationId}", request.ApplicationId);
            throw;
        }
    }

    private async Task CreateSellerProfileAsync(SellerApplication application, CancellationToken cancellationToken)
    {
        // Check if seller profile already exists
        var existingProfile = await context.Set<SellerProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == application.UserId, cancellationToken);

        if (existingProfile is not null)
        {
            logger.LogInformation("Seller profile already exists for user {UserId}", application.UserId);
            return;
        }

        var profile = SellerProfile.Create(
            userId: application.UserId,
            storeName: application.BusinessName,
            commissionRate: sellerConfig.DefaultCommissionRate);

        profile.Verify();
        profile.Activate();

        await context.Set<SellerProfile>().AddAsync(profile, cancellationToken);
        logger.LogInformation("Created seller profile for user {UserId} with store name: {StoreName}",
            application.UserId, application.BusinessName);
    }
}
