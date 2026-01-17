using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Application.Services.Notification;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using UserEntity = Merge.Domain.Modules.Identity.User;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Marketplace.SellerApplication>;

namespace Merge.Application.Seller.Commands.ReviewSellerApplication;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ReviewSellerApplicationCommandHandler(IRepository applicationRepository, UserManager<UserEntity> userManager, IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService, IOptions<SellerSettings> sellerSettings, ILogger<ReviewSellerApplicationCommandHandler> logger) : IRequestHandler<ReviewSellerApplicationCommand, SellerApplicationDto>
{
    private readonly SellerSettings sellerConfig = sellerSettings.Value;

    public async Task<SellerApplicationDto> Handle(ReviewSellerApplicationCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Reviewing seller application {ApplicationId} by reviewer {ReviewerId}, Status: {Status}",
            request.ApplicationId, request.ReviewerId, request.Status);

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            var application = await applicationRepository.GetByIdAsync(request.ApplicationId);
            if (application == null)
            {
                logger.LogWarning("Application review failed - Application {ApplicationId} not found", request.ApplicationId);
                throw new NotFoundException("Başvuru", request.ApplicationId);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            if (request.Status == SellerApplicationStatus.Approved)
            {
                application.Approve(request.ReviewerId);
                await CreateSellerProfileAsync(application, cancellationToken);
                logger.LogInformation("Seller profile created for approved application {ApplicationId}", request.ApplicationId);
            }
            else if (request.Status == SellerApplicationStatus.Rejected)
            {
                application.Reject(request.ReviewerId, request.RejectionReason ?? "Başvuru reddedildi");
            }
            else
            {
                application.Review(request.ReviewerId, request.AdditionalNotes);
            }

            await applicationRepository.UpdateAsync(application);
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Application {ApplicationId} reviewed successfully with status: {Status}",
                request.ApplicationId, request.Status);

            // Send notification email
            var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == application.UserId, cancellationToken);
            if (user != null)
            {
                var subject = request.Status == SellerApplicationStatus.Approved
                    ? "Seller Application Approved!"
                    : $"Seller Application Status: {request.Status}";

                var message = request.Status == SellerApplicationStatus.Approved
                    ? $"Congratulations! Your seller application for {application.BusinessName} has been approved. You can now start selling on our platform."
                    : $"Your seller application status has been updated to: {request.Status}.\n\n" +
                      (string.IsNullOrEmpty(request.RejectionReason) ? "" : $"Reason: {request.RejectionReason}");

                await emailService.SendEmailAsync(user.Email ?? string.Empty, subject, message, true, cancellationToken);
            }

            application = await context.Set<SellerApplication>()
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Reviewer)
                .FirstOrDefaultAsync(a => a.Id == application.Id, cancellationToken);
            
            return mapper.Map<SellerApplicationDto>(application!);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Error reviewing application {ApplicationId}", request.ApplicationId);
            throw;
        }
    }

    private async Task CreateSellerProfileAsync(SellerApplication application, CancellationToken cancellationToken)
    {
        // Check if seller profile already exists
        var existingProfile = await context.Set<SellerProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == application.UserId, cancellationToken);

        if (existingProfile != null)
        {
            logger.LogInformation("Seller profile already exists for user {UserId}", application.UserId);
            return;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        // ✅ BOLUM 12.0: Magic number config'den - SellerSettings kullanımı
        var profile = SellerProfile.Create(
            userId: application.UserId,
            storeName: application.BusinessName,
            commissionRate: sellerConfig.DefaultCommissionRate);

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        profile.Verify();
        profile.Activate();

        await context.Set<SellerProfile>().AddAsync(profile, cancellationToken);
        logger.LogInformation("Created seller profile for user {UserId} with store name: {StoreName}",
            application.UserId, application.BusinessName);
    }
}
