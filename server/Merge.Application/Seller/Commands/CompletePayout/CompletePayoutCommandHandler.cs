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

namespace Merge.Application.Seller.Commands.CompletePayout;

public class CompletePayoutCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IEmailService emailService, ILogger<CompletePayoutCommandHandler> logger) : IRequestHandler<CompletePayoutCommand, bool>
{

    public async Task<bool> Handle(CompletePayoutCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Completing payout. PayoutId: {PayoutId}", request.PayoutId);

        var payout = await context.Set<CommissionPayout>()
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.Id == request.PayoutId, cancellationToken);

        if (payout is null)
        {
            logger.LogWarning("Payout not found. PayoutId: {PayoutId}", request.PayoutId);
            return false;
        }

        payout.Complete();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Send confirmation email
        await emailService.SendEmailAsync(
            payout.Seller?.Email ?? string.Empty,
            $"Payout Completed - {payout.PayoutNumber}",
            $"Your payout of {payout.NetAmount:C} has been completed. Transaction Reference: {payout.TransactionReference}",
            true,
            cancellationToken
        );

        logger.LogInformation("Payout completed. PayoutId: {PayoutId}, PayoutNumber: {PayoutNumber}",
            request.PayoutId, payout.PayoutNumber);

        return true;
    }
}
