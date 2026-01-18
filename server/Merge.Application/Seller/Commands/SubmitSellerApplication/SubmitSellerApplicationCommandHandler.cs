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
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Marketplace.SellerApplication>;

namespace Merge.Application.Seller.Commands.SubmitSellerApplication;

public class SubmitSellerApplicationCommandHandler(IRepository applicationRepository, UserManager<UserEntity> userManager, IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService, ILogger<SubmitSellerApplicationCommandHandler> logger) : IRequestHandler<SubmitSellerApplicationCommand, SellerApplicationDto>
{

    public async Task<SellerApplicationDto> Handle(SubmitSellerApplicationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing seller application submission for user {UserId}, Business: {BusinessName}",
            request.UserId, request.ApplicationDto.BusinessName);

        // Check if user exists
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            logger.LogWarning("Seller application failed - User {UserId} not found", request.UserId);
            throw new NotFoundException("Kullanıcı", request.UserId);
        }

        // Check if user already has an application
        var existingApplication = await context.Set<SellerApplication>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == request.UserId, cancellationToken);

        if (existingApplication is not null && existingApplication.Status != SellerApplicationStatus.Rejected)
        {
            logger.LogWarning("Seller application failed - User {UserId} already has a pending/approved application", request.UserId);
            throw new BusinessException("Zaten bekleyen veya onaylanmış bir başvurunuz var.");
        }

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

        application = await applicationRepository.AddAsync(application);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seller application created successfully for user {UserId}, ApplicationId: {ApplicationId}",
            request.UserId, application.Id);

        // Send confirmation email
        await emailService.SendEmailAsync(
            user.Email ?? string.Empty,
            "Seller Application Received",
            $"Dear {user.FirstName},\n\nWe have received your seller application for {request.ApplicationDto.BusinessName}. " +
            "Our team will review it and get back to you within 2-3 business days.\n\nThank you!",
            true,
            cancellationToken
        );

        application = await context.Set<SellerApplication>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Reviewer)
            .FirstOrDefaultAsync(a => a.Id == application.Id, cancellationToken);
        
        return mapper.Map<SellerApplicationDto>(application!);
    }
}
