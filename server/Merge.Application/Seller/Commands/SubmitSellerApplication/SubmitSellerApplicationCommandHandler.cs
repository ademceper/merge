using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Exceptions;
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

namespace Merge.Application.Seller.Commands.SubmitSellerApplication;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class SubmitSellerApplicationCommandHandler : IRequestHandler<SubmitSellerApplicationCommand, SellerApplicationDto>
{
    private readonly Merge.Application.Interfaces.IRepository<SellerApplication> _applicationRepository;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly ILogger<SubmitSellerApplicationCommandHandler> _logger;

    public SubmitSellerApplicationCommandHandler(
        Merge.Application.Interfaces.IRepository<SellerApplication> applicationRepository,
        UserManager<UserEntity> userManager,
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IEmailService emailService,
        ILogger<SubmitSellerApplicationCommandHandler> logger)
    {
        _applicationRepository = applicationRepository;
        _userManager = userManager;
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<SellerApplicationDto> Handle(SubmitSellerApplicationCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Processing seller application submission for user {UserId}, Business: {BusinessName}",
            request.UserId, request.ApplicationDto.BusinessName);

        // Check if user exists
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            _logger.LogWarning("Seller application failed - User {UserId} not found", request.UserId);
            throw new NotFoundException("Kullanıcı", request.UserId);
        }

        // Check if user already has an application
        var existingApplication = await _context.Set<SellerApplication>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == request.UserId, cancellationToken);

        if (existingApplication != null && existingApplication.Status != SellerApplicationStatus.Rejected)
        {
            _logger.LogWarning("Seller application failed - User {UserId} already has a pending/approved application", request.UserId);
            throw new BusinessException("Zaten bekleyen veya onaylanmış bir başvurunuz var.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        // ✅ ARCHITECTURE: Enum kullanımı (string BusinessType yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
        var application = SellerApplication.Create(
            userId: request.UserId,
            businessName: request.ApplicationDto.BusinessName,
            businessType: request.ApplicationDto.BusinessType,
            taxNumber: request.ApplicationDto.TaxNumber,
            address: request.ApplicationDto.Address,
            city: request.ApplicationDto.City,
            country: request.ApplicationDto.Country,
            postalCode: request.ApplicationDto.PostalCode,
            phoneNumber: request.ApplicationDto.PhoneNumber,
            email: request.ApplicationDto.Email,
            bankName: request.ApplicationDto.BankName,
            bankAccountNumber: request.ApplicationDto.BankAccountNumber,
            bankAccountHolderName: request.ApplicationDto.BankAccountHolderName,
            iban: request.ApplicationDto.IBAN,
            businessDescription: request.ApplicationDto.BusinessDescription,
            productCategories: request.ApplicationDto.ProductCategories ?? string.Empty,
            estimatedMonthlyRevenue: request.ApplicationDto.EstimatedMonthlyRevenue,
            identityDocumentUrl: request.ApplicationDto.IdentityDocumentUrl,
            taxCertificateUrl: request.ApplicationDto.TaxCertificateUrl,
            bankStatementUrl: request.ApplicationDto.BankStatementUrl,
            businessLicenseUrl: request.ApplicationDto.BusinessLicenseUrl);

        // Submit the application
        application.Submit();

        application = await _applicationRepository.AddAsync(application);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seller application created successfully for user {UserId}, ApplicationId: {ApplicationId}",
            request.UserId, application.Id);

        // Send confirmation email
        await _emailService.SendEmailAsync(
            user.Email ?? string.Empty,
            "Seller Application Received",
            $"Dear {user.FirstName},\n\nWe have received your seller application for {request.ApplicationDto.BusinessName}. " +
            "Our team will review it and get back to you within 2-3 business days.\n\nThank you!",
            true,
            cancellationToken
        );

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        application = await _context.Set<SellerApplication>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.User)
            .Include(a => a.Reviewer)
            .FirstOrDefaultAsync(a => a.Id == application.Id, cancellationToken);
        
        return _mapper.Map<SellerApplicationDto>(application!);
    }
}
