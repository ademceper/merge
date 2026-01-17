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
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Marketplace.SellerApplication>;

namespace Merge.Application.Seller.Commands.RejectSellerApplication;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RejectSellerApplicationCommandHandler(IRepository applicationRepository, IDbContext context, IUnitOfWork unitOfWork, IEmailService emailService, ILogger<RejectSellerApplicationCommandHandler> logger) : IRequestHandler<RejectSellerApplicationCommand, bool>
{

    public async Task<bool> Handle(RejectSellerApplicationCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Rejecting seller application {ApplicationId} by reviewer {ReviewerId}, Reason: {Reason}",
            request.ApplicationId, request.ReviewerId, request.Reason);

        var application = await applicationRepository.GetByIdAsync(request.ApplicationId);
        if (application == null)
        {
            logger.LogWarning("Application rejection failed - Application {ApplicationId} not found", request.ApplicationId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        application.Reject(request.ReviewerId, request.Reason);

        await applicationRepository.UpdateAsync(application);
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Send rejection email
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == application.UserId, cancellationToken);
        if (user != null)
        {
            await emailService.SendEmailAsync(
                user.Email ?? string.Empty,
                "Seller Application Update",
                $"We regret to inform you that your seller application for {application.BusinessName} " +
                $"has been rejected.\n\nReason: {request.Reason}\n\n" +
                "You can submit a new application after addressing the concerns mentioned above.",
                true,
                cancellationToken
            );
        }

        logger.LogInformation("Application {ApplicationId} rejected successfully", request.ApplicationId);
        return true;
    }
}
