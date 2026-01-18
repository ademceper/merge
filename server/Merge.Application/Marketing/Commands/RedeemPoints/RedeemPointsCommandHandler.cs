using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.RedeemPoints;

public class RedeemPointsCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<RedeemPointsCommandHandler> logger,
    IOptions<MarketingSettings> marketingSettings) : IRequestHandler<RedeemPointsCommand, bool>
{
    public async Task<bool> Handle(RedeemPointsCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Redeeming points. UserId: {UserId}, Points: {Points}", request.UserId, request.Points);

        var account = await context.Set<LoyaltyAccount>()
            .FirstOrDefaultAsync(a => a.UserId == request.UserId, cancellationToken);

        if (account is null)
        {
            logger.LogWarning("LoyaltyAccount not found. UserId: {UserId}", request.UserId);
            throw new NotFoundException("Sadakat hesabı", request.UserId);
        }

        try
        {
            account.DeductPoints(request.Points, $"Redeemed {request.Points} points");
        }
        catch (DomainException ex)
        {
            logger.LogWarning("Insufficient points. UserId: {UserId}, Points: {Points}, Error: {Error}", 
                request.UserId, request.Points, ex.Message);
            throw new BusinessException(ex.Message);
        }

        var transaction = LoyaltyTransaction.Create(
            request.UserId,
            account.Id,
            -request.Points,
            LoyaltyTransactionType.Redeem,
            $"Redeemed {request.Points} points",
            DateTime.UtcNow.AddYears(marketingSettings.Value.PointsExpiryYears),
            request.OrderId,
            null);

        await context.Set<LoyaltyTransaction>().AddAsync(transaction, cancellationToken);
        
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Points redeemed successfully. UserId: {UserId}, Points: {Points}", request.UserId, request.Points);

        return true;
    }
}
