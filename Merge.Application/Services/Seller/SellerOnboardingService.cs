using AutoMapper;
using Merge.Application.Services.Notification;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using UserEntity = Merge.Domain.Entities.User;
using OrderEntity = Merge.Domain.Entities.Order;
using ProductEntity = Merge.Domain.Entities.Product;
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

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
namespace Merge.Application.Services.Seller;

public class SellerOnboardingService : ISellerOnboardingService
{
    private readonly IRepository<SellerApplication> _applicationRepository;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SellerOnboardingService> _logger;
    private readonly SellerSettings _sellerSettings;
    private readonly PaginationSettings _paginationSettings;

    public SellerOnboardingService(
        IRepository<SellerApplication> applicationRepository,
        UserManager<UserEntity> userManager,
        IDbContext context,
        IMapper mapper,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<SellerOnboardingService> logger,
        IOptions<SellerSettings> sellerSettings,
        IOptions<PaginationSettings> paginationSettings)
    {
        _applicationRepository = applicationRepository;
        _userManager = userManager;
        _context = context;
        _mapper = mapper;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _sellerSettings = sellerSettings.Value;
        _paginationSettings = paginationSettings.Value;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<SellerApplicationDto> SubmitApplicationAsync(Guid userId, CreateSellerApplicationDto applicationDto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Processing seller application submission for user {UserId}, Business: {BusinessName}",
            userId, applicationDto.BusinessName);

        // Check if user exists
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            _logger.LogWarning("Seller application failed - User {UserId} not found", userId);
            throw new NotFoundException("Kullanıcı", userId);
        }

        // Check if user already has an application
        var existingApplication = await _context.Set<SellerApplication>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);

        if (existingApplication != null && existingApplication.Status != SellerApplicationStatus.Rejected)
        {
            _logger.LogWarning("Seller application failed - User {UserId} already has a pending/approved application", userId);
            throw new BusinessException("Zaten bekleyen veya onaylanmış bir başvurunuz var.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        // ✅ ARCHITECTURE: Enum kullanımı (string BusinessType yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
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

        application = await _applicationRepository.AddAsync(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seller application created successfully for user {UserId}, ApplicationId: {ApplicationId}",
            userId, application.Id);

        // Send confirmation email
        await _emailService.SendEmailAsync(
            user.Email ?? string.Empty,
            "Seller Application Received",
            $"Dear {user.FirstName},\n\nWe have received your seller application for {applicationDto.BusinessName}. " +
            "Our team will review it and get back to you within 2-3 business days.\n\nThank you!",
            true,
            cancellationToken
        );

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        application = await _context.Set<SellerApplication>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Reviewer)
            .FirstOrDefaultAsync(a => a.Id == application.Id, cancellationToken);
        
        return _mapper.Map<SellerApplicationDto>(application);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SellerApplicationDto?> GetApplicationByIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        var application = await _context.Set<SellerApplication>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Reviewer)
            .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

        return application == null ? null : _mapper.Map<SellerApplicationDto>(application);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SellerApplicationDto?> GetUserApplicationAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var application = await _context.Set<SellerApplication>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Reviewer)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return application == null ? null : _mapper.Map<SellerApplicationDto>(application);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<SellerApplicationDto>> GetAllApplicationsAsync(
        SellerApplicationStatus? status = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic number config'den - PaginationSettings kullanımı
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ FIX: Explicitly type as IQueryable to avoid IIncludableQueryable type mismatch
        IQueryable<SellerApplication> query = _context.Set<SellerApplication>()
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

        var applicationDtos = _mapper.Map<IEnumerable<SellerApplicationDto>>(applications).ToList();

        return new PagedResult<SellerApplicationDto>
        {
            Items = applicationDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<SellerApplicationDto> ReviewApplicationAsync(
        Guid applicationId,
        ReviewSellerApplicationDto reviewDto,
        Guid reviewerId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Reviewing seller application {ApplicationId} by reviewer {ReviewerId}, Status: {Status}",
            applicationId, reviewerId, reviewDto.Status);

        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var application = await _applicationRepository.GetByIdAsync(applicationId);
            if (application == null)
            {
                _logger.LogWarning("Application review failed - Application {ApplicationId} not found", applicationId);
                throw new NotFoundException("Başvuru", applicationId);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            if (reviewDto.Status == SellerApplicationStatus.Approved)
            {
                application.Approve(reviewerId);
                await CreateSellerProfileAsync(application, cancellationToken);
                _logger.LogInformation("Seller profile created for approved application {ApplicationId}", applicationId);
            }
            else if (reviewDto.Status == SellerApplicationStatus.Rejected)
            {
                application.Reject(reviewerId, reviewDto.RejectionReason ?? "Başvuru reddedildi");
            }
            else if (reviewDto.Status == SellerApplicationStatus.UnderReview)
            {
                application.Review(reviewerId, reviewDto.AdditionalNotes);
            }

            await _applicationRepository.UpdateAsync(application);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Application {ApplicationId} reviewed successfully with status: {Status}",
                applicationId, reviewDto.Status);

            // Send notification email
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == application.UserId, cancellationToken);
            if (user != null)
            {
                var subject = reviewDto.Status == SellerApplicationStatus.Approved
                    ? "Seller Application Approved!"
                    : $"Seller Application Status: {reviewDto.Status}";

                var message = reviewDto.Status == SellerApplicationStatus.Approved
                    ? $"Congratulations! Your seller application for {application.BusinessName} has been approved. You can now start selling on our platform."
                    : $"Your seller application status has been updated to: {reviewDto.Status}.\n\n" +
                      (string.IsNullOrEmpty(reviewDto.RejectionReason) ? "" : $"Reason: {reviewDto.RejectionReason}");

                await _emailService.SendEmailAsync(user.Email ?? string.Empty, subject, message, true, cancellationToken);
            }

            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            application = await _context.Set<SellerApplication>()
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Reviewer)
                .FirstOrDefaultAsync(a => a.Id == application.Id, cancellationToken);
            
            return _mapper.Map<SellerApplicationDto>(application);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error reviewing application {ApplicationId}", applicationId);
            throw;
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<bool> ApproveApplicationAsync(Guid applicationId, Guid reviewerId, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Approving seller application {ApplicationId} by reviewer {ReviewerId}",
            applicationId, reviewerId);

        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var application = await _applicationRepository.GetByIdAsync(applicationId);
            if (application == null)
            {
                _logger.LogWarning("Application approval failed - Application {ApplicationId} not found", applicationId);
                return false;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            application.Approve(reviewerId);

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

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Application {ApplicationId} approved successfully", applicationId);
            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error approving application {ApplicationId}", applicationId);
            throw;
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<bool> RejectApplicationAsync(Guid applicationId, string reason, Guid reviewerId, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Rejecting seller application {ApplicationId} by reviewer {ReviewerId}, Reason: {Reason}",
            applicationId, reviewerId, reason);

        var application = await _applicationRepository.GetByIdAsync(applicationId);
        if (application == null)
        {
            _logger.LogWarning("Application rejection failed - Application {ApplicationId} not found", applicationId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        application.Reject(reviewerId, reason);

        await _applicationRepository.UpdateAsync(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send rejection email
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == application.UserId, cancellationToken);
        if (user != null)
        {
            await _emailService.SendEmailAsync(
                user.Email ?? string.Empty,
                "Seller Application Update",
                $"We regret to inform you that your seller application for {application.BusinessName} " +
                $"has been rejected.\n\nReason: {reason}\n\n" +
                "You can submit a new application after addressing the concerns mentioned above.",
                true,
                cancellationToken
            );
        }

        _logger.LogInformation("Application {ApplicationId} rejected successfully", applicationId);
        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SellerOnboardingStatsDto> GetOnboardingStatsAsync(CancellationToken cancellationToken = default)
    {
        var stats = await _context.Set<SellerApplication>()
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
        var approvedThisMonth = await _context.Set<SellerApplication>()
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
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    private async Task CreateSellerProfileAsync(SellerApplication application, CancellationToken cancellationToken = default)
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
            commissionRate: _sellerSettings.DefaultCommissionRate);

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        profile.Verify();
        profile.Activate();

        await _context.Set<SellerProfile>().AddAsync(profile, cancellationToken);
        _logger.LogInformation("Created seller profile for user {UserId} with store name: {StoreName}",
            application.UserId, application.BusinessName);
    }
}
