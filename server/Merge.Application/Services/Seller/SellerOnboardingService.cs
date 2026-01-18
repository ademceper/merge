using AutoMapper;
using Merge.Application.Services.Notification;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using UserEntity = Merge.Domain.Modules.Identity.User;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces.Seller;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Application.DTOs.Seller;
using Microsoft.Extensions.Logging;
using Merge.Application.Common;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Marketplace.SellerApplication>;

namespace Merge.Application.Services.Seller;

public class SellerOnboardingService(IRepository applicationRepository, UserManager<UserEntity> userManager, IDbContext context, IMapper mapper, IEmailService emailService, IUnitOfWork unitOfWork, ILogger<SellerOnboardingService> logger, IOptions<SellerSettings> sellerSettings, IOptions<PaginationSettings> paginationSettings) : ISellerOnboardingService
{
    private readonly SellerSettings sellerConfig = sellerSettings.Value;
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<SellerApplicationDto> SubmitApplicationAsync(Guid userId, CreateSellerApplicationDto applicationDto, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Processing seller application submission for user {UserId}, Business: {BusinessName}",
            userId, applicationDto.BusinessName);

        // Check if user exists
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            logger.LogWarning("Seller application failed - User {UserId} not found", userId);
            throw new NotFoundException("Kullanıcı", userId);
        }

        // Check if user already has an application
        var existingApplication = await context.Set<SellerApplication>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);

        if (existingApplication is not null && existingApplication.Status != SellerApplicationStatus.Rejected)
        {
            logger.LogWarning("Seller application failed - User {UserId} already has a pending/approved application", userId);
            throw new BusinessException("Zaten bekleyen veya onaylanmış bir başvurunuz var.");
        }

        var application = SellerApplication.Create(
            userId: userId,
            businessName: applicationDto.BusinessName,
            businessType: applicationDto.BusinessType,
            taxNumber: applicationDto.TaxNumber,
            address: applicationDto.Address,
            city: applicationDto.City,
            country: applicationDto.Country,
            postalCode: applicationDto.PostalCode,
            phoneNumber: applicationDto.PhoneNumber,
            email: applicationDto.Email,
            bankName: applicationDto.BankName,
            bankAccountNumber: applicationDto.BankAccountNumber,
            bankAccountHolderName: applicationDto.BankAccountHolderName,
            iban: applicationDto.IBAN,
            businessDescription: applicationDto.BusinessDescription,
            productCategories: applicationDto.ProductCategories,
            estimatedMonthlyRevenue: applicationDto.EstimatedMonthlyRevenue,
            identityDocumentUrl: applicationDto.IdentityDocumentUrl,
            taxCertificateUrl: applicationDto.TaxCertificateUrl,
            bankStatementUrl: applicationDto.BankStatementUrl,
            businessLicenseUrl: applicationDto.BusinessLicenseUrl
        );

        application = await applicationRepository.AddAsync(application);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seller application created successfully for user {UserId}, ApplicationId: {ApplicationId}",
            userId, application.Id);

        // Send confirmation email
        await emailService.SendEmailAsync(
            user.Email ?? string.Empty,
            "Seller Application Received",
            $"Dear {user.FirstName},\n\nWe have received your seller application for {applicationDto.BusinessName}. " +
            "Our team will review it and get back to you within 2-3 business days.\n\nThank you!",
            true,
            cancellationToken
        );

        application = await context.Set<SellerApplication>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.User)
            .Include(a => a.Reviewer)
            .FirstOrDefaultAsync(a => a.Id == application.Id, cancellationToken);
        
        return mapper.Map<SellerApplicationDto>(application);
    }

    public async Task<SellerApplicationDto?> GetApplicationByIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        var application = await context.Set<SellerApplication>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.User)
            .Include(a => a.Reviewer)
            .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

        return application is null ? null : mapper.Map<SellerApplicationDto>(application);
    }

    public async Task<SellerApplicationDto?> GetUserApplicationAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var application = await context.Set<SellerApplication>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.User)
            .Include(a => a.Reviewer)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return application is null ? null : mapper.Map<SellerApplicationDto>(application);
    }

    public async Task<PagedResult<SellerApplicationDto>> GetAllApplicationsAsync(
        SellerApplicationStatus? status = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > paginationConfig.MaxPageSize) pageSize = paginationConfig.MaxPageSize;
        if (page < 1) page = 1;

        IQueryable<SellerApplication> query = context.Set<SellerApplication>()
            .AsNoTracking()
            .Include(a => a.User);

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var applications = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var applicationDtos = mapper.Map<IEnumerable<SellerApplicationDto>>(applications).ToList();

        return new PagedResult<SellerApplicationDto>
        {
            Items = applicationDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<SellerApplicationDto> ReviewApplicationAsync(
        Guid applicationId,
        ReviewSellerApplicationDto reviewDto,
        Guid reviewerId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Reviewing seller application {ApplicationId} by reviewer {ReviewerId}, Status: {Status}",
            applicationId, reviewerId, reviewDto.Status);

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            var application = await applicationRepository.GetByIdAsync(applicationId);
            if (application is null)
            {
                logger.LogWarning("Application review failed - Application {ApplicationId} not found", applicationId);
                throw new NotFoundException("Başvuru", applicationId);
            }

            if (reviewDto.Status == SellerApplicationStatus.Approved)
            {
                application.Approve(reviewerId);
                await CreateSellerProfileAsync(application, cancellationToken);
                logger.LogInformation("Seller profile created for approved application {ApplicationId}", applicationId);
            }
            else if (reviewDto.Status == SellerApplicationStatus.Rejected)
            {
                application.Reject(reviewerId, reviewDto.RejectionReason ?? "Başvuru reddedildi");
            }
            else if (reviewDto.Status == SellerApplicationStatus.UnderReview)
            {
                application.Review(reviewerId, reviewDto.AdditionalNotes);
            }

            await applicationRepository.UpdateAsync(application);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Application {ApplicationId} reviewed successfully with status: {Status}",
                applicationId, reviewDto.Status);

            // Send notification email
            var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == application.UserId, cancellationToken);
            if (user is not null)
            {
                var subject = reviewDto.Status == SellerApplicationStatus.Approved
                    ? "Seller Application Approved!"
                    : $"Seller Application Status: {reviewDto.Status}";

                var message = reviewDto.Status == SellerApplicationStatus.Approved
                    ? $"Congratulations! Your seller application for {application.BusinessName} has been approved. You can now start selling on our platform."
                    : $"Your seller application status has been updated to: {reviewDto.Status}.\n\n" +
                      (string.IsNullOrEmpty(reviewDto.RejectionReason) ? "" : $"Reason: {reviewDto.RejectionReason}");

                await emailService.SendEmailAsync(user.Email ?? string.Empty, subject, message, true, cancellationToken);
            }

            application = await context.Set<SellerApplication>()
                .AsNoTracking()
                .AsSplitQuery()
                .Include(a => a.User)
                .Include(a => a.Reviewer)
                .FirstOrDefaultAsync(a => a.Id == application.Id, cancellationToken);
            
            return mapper.Map<SellerApplicationDto>(application);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Error reviewing application {ApplicationId}", applicationId);
            throw;
        }
    }

    public async Task<bool> ApproveApplicationAsync(Guid applicationId, Guid reviewerId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Approving seller application {ApplicationId} by reviewer {ReviewerId}",
            applicationId, reviewerId);

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            var application = await applicationRepository.GetByIdAsync(applicationId);
            if (application is null)
            {
                logger.LogWarning("Application approval failed - Application {ApplicationId} not found", applicationId);
                return false;
            }

            application.Approve(reviewerId);

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

                // Update user role to Seller
                var currentRoles = await userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    await userManager.RemoveFromRolesAsync(user, currentRoles);
                }
                await userManager.AddToRoleAsync(user, "Seller");
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Application {ApplicationId} approved successfully", applicationId);
            return true;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Error approving application {ApplicationId}", applicationId);
            throw;
        }
    }

    public async Task<bool> RejectApplicationAsync(Guid applicationId, string reason, Guid reviewerId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Rejecting seller application {ApplicationId} by reviewer {ReviewerId}, Reason: {Reason}",
            applicationId, reviewerId, reason);

        var application = await applicationRepository.GetByIdAsync(applicationId);
        if (application is null)
        {
            logger.LogWarning("Application rejection failed - Application {ApplicationId} not found", applicationId);
            return false;
        }

        application.Reject(reviewerId, reason);

        await applicationRepository.UpdateAsync(application);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Send rejection email
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == application.UserId, cancellationToken);
        if (user is not null)
        {
            await emailService.SendEmailAsync(
                user.Email ?? string.Empty,
                "Seller Application Update",
                $"We regret to inform you that your seller application for {application.BusinessName} " +
                $"has been rejected.\n\nReason: {reason}\n\n" +
                "You can submit a new application after addressing the concerns mentioned above.",
                true,
                cancellationToken
            );
        }

        logger.LogInformation("Application {ApplicationId} rejected successfully", applicationId);
        return true;
    }

    public async Task<SellerOnboardingStatsDto> GetOnboardingStatsAsync(CancellationToken cancellationToken = default)
    {
        var stats = await context.Set<SellerApplication>()
            .AsNoTracking()
            .GroupBy(a => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Pending = g.Count(a => a.Status == SellerApplicationStatus.Pending),
                Approved = g.Count(a => a.Status == SellerApplicationStatus.Approved),
                Rejected = g.Count(a => a.Status == SellerApplicationStatus.Rejected)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var thisMonth = DateTime.UtcNow.AddMonths(-1);
        var approvedThisMonth = await context.Set<SellerApplication>()
            .AsNoTracking()
            .CountAsync(a => a.Status == SellerApplicationStatus.Approved &&
                           a.ApprovedAt >= thisMonth, cancellationToken);

        var total = stats?.Total ?? 0;
        var approved = stats?.Approved ?? 0;

        return new SellerOnboardingStatsDto
        {
            TotalApplications = total,
            PendingApplications = stats?.Pending ?? 0,
            ApprovedApplications = approved,
            RejectedApplications = stats?.Rejected ?? 0,
            ApprovedThisMonth = approvedThisMonth,
            ApprovalRate = total > 0 ? (approved * 100.0m / total) : 0
        };
    }

    // Helper methods
    private async Task CreateSellerProfileAsync(SellerApplication application, CancellationToken cancellationToken = default)
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
