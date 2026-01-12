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

namespace Merge.Application.Seller.Commands.ReviewSellerApplication;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ReviewSellerApplicationCommandHandler : IRequestHandler<ReviewSellerApplicationCommand, SellerApplicationDto>
{
    private readonly Merge.Application.Interfaces.IRepository<SellerApplication> _applicationRepository;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IOptions<SellerSettings> _sellerSettings;
    private readonly ILogger<ReviewSellerApplicationCommandHandler> _logger;

    public ReviewSellerApplicationCommandHandler(
        Merge.Application.Interfaces.IRepository<SellerApplication> applicationRepository,
        UserManager<UserEntity> userManager,
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IEmailService emailService,
        IOptions<SellerSettings> sellerSettings,
        ILogger<ReviewSellerApplicationCommandHandler> logger)
    {
        _applicationRepository = applicationRepository;
        _userManager = userManager;
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _emailService = emailService;
        _sellerSettings = sellerSettings;
        _logger = logger;
    }

    public async Task<SellerApplicationDto> Handle(ReviewSellerApplicationCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Reviewing seller application {ApplicationId} by reviewer {ReviewerId}, Status: {Status}",
            request.ApplicationId, request.ReviewerId, request.Status);

        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var application = await _applicationRepository.GetByIdAsync(request.ApplicationId);
            if (application == null)
            {
                _logger.LogWarning("Application review failed - Application {ApplicationId} not found", request.ApplicationId);
                throw new NotFoundException("Başvuru", request.ApplicationId);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            if (request.Status == SellerApplicationStatus.Approved)
            {
                application.Approve(request.ReviewerId);
                await CreateSellerProfileAsync(application, cancellationToken);
                _logger.LogInformation("Seller profile created for approved application {ApplicationId}", request.ApplicationId);
            }
            else if (request.Status == SellerApplicationStatus.Rejected)
            {
                application.Reject(request.ReviewerId, request.RejectionReason ?? "Başvuru reddedildi");
            }
            else
            {
                application.Review(request.ReviewerId, request.AdditionalNotes);
            }

            await _applicationRepository.UpdateAsync(application);
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Application {ApplicationId} reviewed successfully with status: {Status}",
                request.ApplicationId, request.Status);

            // Send notification email
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == application.UserId, cancellationToken);
            if (user != null)
            {
                var subject = request.Status == SellerApplicationStatus.Approved
                    ? "Seller Application Approved!"
                    : $"Seller Application Status: {request.Status}";

                var message = request.Status == SellerApplicationStatus.Approved
                    ? $"Congratulations! Your seller application for {application.BusinessName} has been approved. You can now start selling on our platform."
                    : $"Your seller application status has been updated to: {request.Status}.\n\n" +
                      (string.IsNullOrEmpty(request.RejectionReason) ? "" : $"Reason: {request.RejectionReason}");

                await _emailService.SendEmailAsync(user.Email ?? string.Empty, subject, message, true, cancellationToken);
            }

            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            application = await _context.Set<SellerApplication>()
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Reviewer)
                .FirstOrDefaultAsync(a => a.Id == application.Id, cancellationToken);
            
            return _mapper.Map<SellerApplicationDto>(application!);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error reviewing application {ApplicationId}", request.ApplicationId);
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
