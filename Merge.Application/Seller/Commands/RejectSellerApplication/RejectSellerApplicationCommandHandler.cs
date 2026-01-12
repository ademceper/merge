using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Services.Notification;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.RejectSellerApplication;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RejectSellerApplicationCommandHandler : IRequestHandler<RejectSellerApplicationCommand, bool>
{
    private readonly Merge.Application.Interfaces.IRepository<SellerApplication> _applicationRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<RejectSellerApplicationCommandHandler> _logger;

    public RejectSellerApplicationCommandHandler(
        Merge.Application.Interfaces.IRepository<SellerApplication> applicationRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<RejectSellerApplicationCommandHandler> logger)
    {
        _applicationRepository = applicationRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> Handle(RejectSellerApplicationCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Rejecting seller application {ApplicationId} by reviewer {ReviewerId}, Reason: {Reason}",
            request.ApplicationId, request.ReviewerId, request.Reason);

        var application = await _applicationRepository.GetByIdAsync(request.ApplicationId);
        if (application == null)
        {
            _logger.LogWarning("Application rejection failed - Application {ApplicationId} not found", request.ApplicationId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        application.Reject(request.ReviewerId, request.Reason);

        await _applicationRepository.UpdateAsync(application);
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send rejection email
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == application.UserId, cancellationToken);
        if (user != null)
        {
            await _emailService.SendEmailAsync(
                user.Email ?? string.Empty,
                "Seller Application Update",
                $"We regret to inform you that your seller application for {application.BusinessName} " +
                $"has been rejected.\n\nReason: {request.Reason}\n\n" +
                "You can submit a new application after addressing the concerns mentioned above.",
                true,
                cancellationToken
            );
        }

        _logger.LogInformation("Application {ApplicationId} rejected successfully", request.ApplicationId);
        return true;
    }
}
