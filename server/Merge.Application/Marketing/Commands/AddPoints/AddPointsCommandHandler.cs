using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.AddPoints;

public class AddPointsCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<AddPointsCommandHandler> logger,
    IOptions<MarketingSettings> marketingSettings) : IRequestHandler<AddPointsCommand, bool>
{
    public async Task<bool> Handle(AddPointsCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding points. UserId: {UserId}, Points: {Points}, Type: {Type}, Description: {Description}",
            request.UserId, request.Points, request.Type, request.Description);

        var account = await context.Set<LoyaltyAccount>()
            .FirstOrDefaultAsync(a => a.UserId == request.UserId, cancellationToken);

        if (account == null)
        {
            logger.LogWarning("LoyaltyAccount not found. UserId: {UserId}", request.UserId);
            throw new NotFoundException("Sadakat hesabı", request.UserId);
        }

        account.AddPoints(request.Points, request.Description);

        var transactionType = Enum.TryParse<LoyaltyTransactionType>(request.Type, true, out var type) 
            ? type 
            : LoyaltyTransactionType.Purchase;
        
        var expiresAt = DateTime.UtcNow.AddYears(marketingSettings.Value.PointsExpiryYears);
        var transaction = LoyaltyTransaction.Create(
            request.UserId,
            account.Id,
            request.Points,
            transactionType,
            request.Description,
            expiresAt,
            request.OrderId,
            null);

        await context.Set<LoyaltyTransaction>().AddAsync(transaction, cancellationToken);
        
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Points added successfully. UserId: {UserId}, Points: {Points}, NewBalance: {NewBalance}",
            request.UserId, request.Points, account.PointsBalance);

        return true;
    }
}
