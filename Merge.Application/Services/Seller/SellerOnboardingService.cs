using AutoMapper;
using Merge.Application.Services.Notification;
using Merge.Application.Interfaces.User;
using UserEntity = Merge.Domain.Entities.User;
using OrderEntity = Merge.Domain.Entities.Order;
using ProductEntity = Merge.Domain.Entities.Product;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.Seller;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Seller;
using Microsoft.Extensions.Logging;

namespace Merge.Application.Services.Seller;

public class SellerOnboardingService : ISellerOnboardingService
{
    private readonly IRepository<SellerApplication> _applicationRepository;
    private readonly UserManager<UserEntity> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SellerOnboardingService> _logger;

    public SellerOnboardingService(
        IRepository<SellerApplication> applicationRepository,
        UserManager<UserEntity> userManager,
        ApplicationDbContext context,
        IMapper mapper,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<SellerOnboardingService> logger)
    {
        _applicationRepository = applicationRepository;
        _userManager = userManager;
        _context = context;
        _mapper = mapper;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<SellerApplicationDto> SubmitApplicationAsync(Guid userId, CreateSellerApplicationDto applicationDto)
    {
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
            .FirstOrDefaultAsync(a => a.UserId == userId);

        if (existingApplication != null && existingApplication.Status != SellerApplicationStatus.Rejected)
        {
            _logger.LogWarning("Seller application failed - User {UserId} already has a pending/approved application", userId);
            throw new BusinessException("Zaten bekleyen veya onaylanmış bir başvurunuz var.");
        }

        var application = _mapper.Map<SellerApplication>(applicationDto);
        application.UserId = userId;
        application.Status = SellerApplicationStatus.Pending;

        application = await _applicationRepository.AddAsync(application);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Seller application created successfully for user {UserId}, ApplicationId: {ApplicationId}",
            userId, application.Id);

        // Send confirmation email
        await _emailService.SendEmailAsync(
            user.Email ?? string.Empty,
            "Seller Application Received",
            $"Dear {user.FirstName},\n\nWe have received your seller application for {applicationDto.BusinessName}. " +
            "Our team will review it and get back to you within 2-3 business days.\n\nThank you!"
        );

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        application = await _context.Set<SellerApplication>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Reviewer)
            .FirstOrDefaultAsync(a => a.Id == application.Id);
        
        return _mapper.Map<SellerApplicationDto>(application);
    }

    public async Task<SellerApplicationDto?> GetApplicationByIdAsync(Guid applicationId)
    {
        var application = await _context.Set<SellerApplication>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Reviewer)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        return application == null ? null : _mapper.Map<SellerApplicationDto>(application);
    }

    public async Task<SellerApplicationDto?> GetUserApplicationAsync(Guid userId)
    {
        var application = await _context.Set<SellerApplication>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Reviewer)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync();

        return application == null ? null : _mapper.Map<SellerApplicationDto>(application);
    }

    public async Task<IEnumerable<SellerApplicationDto>> GetAllApplicationsAsync(
        SellerApplicationStatus? status = null,
        int page = 1,
        int pageSize = 20)
    {
        // ✅ FIX: Explicitly type as IQueryable to avoid IIncludableQueryable type mismatch
        IQueryable<SellerApplication> query = _context.Set<SellerApplication>()
            .AsNoTracking()
            .Include(a => a.User);

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        var applications = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<IEnumerable<SellerApplicationDto>>(applications);
    }

    public async Task<SellerApplicationDto> ReviewApplicationAsync(
        Guid applicationId,
        ReviewSellerApplicationDto reviewDto,
        Guid reviewerId)
    {
        _logger.LogInformation("Reviewing seller application {ApplicationId} by reviewer {ReviewerId}, Status: {Status}",
            applicationId, reviewerId, reviewDto.Status);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var application = await _applicationRepository.GetByIdAsync(applicationId);
            if (application == null)
            {
                _logger.LogWarning("Application review failed - Application {ApplicationId} not found", applicationId);
                throw new NotFoundException("Başvuru", applicationId);
            }

            application.Status = reviewDto.Status;
            application.RejectionReason = reviewDto.RejectionReason;
            application.AdditionalNotes = reviewDto.AdditionalNotes;
            application.ReviewedBy = reviewerId;
            application.ReviewedAt = DateTime.UtcNow;

            if (reviewDto.Status == SellerApplicationStatus.Approved)
            {
                application.ApprovedAt = DateTime.UtcNow;
                await CreateSellerProfileAsync(application);
                _logger.LogInformation("Seller profile created for approved application {ApplicationId}", applicationId);
            }

            await _applicationRepository.UpdateAsync(application);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Application {ApplicationId} reviewed successfully with status: {Status}",
                applicationId, reviewDto.Status);

            // Send notification email
            var user = await _context.Set<UserEntity>().AsNoTracking().FirstOrDefaultAsync(u => u.Id == application.UserId);
            if (user != null)
            {
                var subject = reviewDto.Status == SellerApplicationStatus.Approved
                    ? "Seller Application Approved!"
                    : $"Seller Application Status: {reviewDto.Status}";

                var message = reviewDto.Status == SellerApplicationStatus.Approved
                    ? $"Congratulations! Your seller application for {application.BusinessName} has been approved. You can now start selling on our platform."
                    : $"Your seller application status has been updated to: {reviewDto.Status}.\n\n" +
                      (string.IsNullOrEmpty(reviewDto.RejectionReason) ? "" : $"Reason: {reviewDto.RejectionReason}");

                await _emailService.SendEmailAsync(user.Email ?? string.Empty, subject, message);
            }

            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            application = await _context.Set<SellerApplication>()
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Reviewer)
                .FirstOrDefaultAsync(a => a.Id == application.Id);
            
            return _mapper.Map<SellerApplicationDto>(application);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error reviewing application {ApplicationId}", applicationId);
            throw;
        }
    }

    public async Task<bool> ApproveApplicationAsync(Guid applicationId, Guid reviewerId)
    {
        _logger.LogInformation("Approving seller application {ApplicationId} by reviewer {ReviewerId}",
            applicationId, reviewerId);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var application = await _applicationRepository.GetByIdAsync(applicationId);
            if (application == null)
            {
                _logger.LogWarning("Application approval failed - Application {ApplicationId} not found", applicationId);
                return false;
            }

            application.Status = SellerApplicationStatus.Approved;
            application.ReviewedBy = reviewerId;
            application.ReviewedAt = DateTime.UtcNow;
            application.ApprovedAt = DateTime.UtcNow;

            await _applicationRepository.UpdateAsync(application);
            await CreateSellerProfileAsync(application);

            // Send approval email
            var user = await _context.Set<UserEntity>().AsNoTracking().FirstOrDefaultAsync(u => u.Id == application.UserId);
            if (user != null)
            {
                await _emailService.SendEmailAsync(
                    user.Email ?? string.Empty,
                    "Seller Application Approved!",
                    $"Congratulations! Your seller application for {application.BusinessName} has been approved. " +
                    "You can now start selling on our platform."
                );

                // Update user role to Seller
                var sellerRole = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Name == "Seller");
                if (sellerRole != null)
                {
                    var existingRoles = await _context.UserRoles.Where(ur => ur.UserId == user.Id).ToListAsync();
                    _context.UserRoles.RemoveRange(existingRoles);

                    await _context.UserRoles.AddAsync(new Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>
                    {
                        UserId = user.Id,
                        RoleId = sellerRole.Id
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Application {ApplicationId} approved successfully", applicationId);
            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error approving application {ApplicationId}", applicationId);
            throw;
        }
    }

    public async Task<bool> RejectApplicationAsync(Guid applicationId, string reason, Guid reviewerId)
    {
        _logger.LogInformation("Rejecting seller application {ApplicationId} by reviewer {ReviewerId}, Reason: {Reason}",
            applicationId, reviewerId, reason);

        var application = await _applicationRepository.GetByIdAsync(applicationId);
        if (application == null)
        {
            _logger.LogWarning("Application rejection failed - Application {ApplicationId} not found", applicationId);
            return false;
        }

        application.Status = SellerApplicationStatus.Rejected;
        application.RejectionReason = reason;
        application.ReviewedBy = reviewerId;
        application.ReviewedAt = DateTime.UtcNow;

        await _applicationRepository.UpdateAsync(application);
        await _unitOfWork.SaveChangesAsync();

        // Send rejection email
        var user = await _context.Set<UserEntity>().AsNoTracking().FirstOrDefaultAsync(u => u.Id == application.UserId);
        if (user != null)
        {
            await _emailService.SendEmailAsync(
                user.Email ?? string.Empty,
                "Seller Application Update",
                $"We regret to inform you that your seller application for {application.BusinessName} " +
                $"has been rejected.\n\nReason: {reason}\n\n" +
                "You can submit a new application after addressing the concerns mentioned above."
            );
        }

        _logger.LogInformation("Application {ApplicationId} rejected successfully", applicationId);
        return true;
    }

    public async Task<SellerOnboardingStatsDto> GetOnboardingStatsAsync()
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
            .FirstOrDefaultAsync();

        var thisMonth = DateTime.UtcNow.AddMonths(-1);
        var approvedThisMonth = await _context.Set<SellerApplication>()
            .AsNoTracking()
            .CountAsync(a => a.Status == SellerApplicationStatus.Approved &&
                           a.ApprovedAt >= thisMonth);

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
    private async Task CreateSellerProfileAsync(SellerApplication application)
    {
        // Check if seller profile already exists
        var existingProfile = await _context.SellerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == application.UserId);

        if (existingProfile != null)
        {
            _logger.LogInformation("Seller profile already exists for user {UserId}", application.UserId);
            return;
        }

        var profile = new SellerProfile
        {
            UserId = application.UserId,
            StoreName = application.BusinessName,
            CommissionRate = 15, // Default commission rate
            Status = SellerStatus.Approved,
            VerifiedAt = DateTime.UtcNow
        };

        await _context.SellerProfiles.AddAsync(profile);
        _logger.LogInformation("Created seller profile for user {UserId} with store name: {StoreName}",
            application.UserId, application.BusinessName);
    }
}
