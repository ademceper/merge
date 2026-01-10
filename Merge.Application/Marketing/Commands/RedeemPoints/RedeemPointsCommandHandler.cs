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

namespace Merge.Application.Marketing.Commands.RedeemPoints;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RedeemPointsCommandHandler : IRequestHandler<RedeemPointsCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RedeemPointsCommandHandler> _logger;
    private readonly MarketingSettings _marketingSettings;

    public RedeemPointsCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<RedeemPointsCommandHandler> logger,
        IOptions<MarketingSettings> marketingSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _marketingSettings = marketingSettings.Value;
    }

    public async Task<bool> Handle(RedeemPointsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Redeeming points. UserId: {UserId}, Points: {Points}", request.UserId, request.Points);

        var account = await _context.Set<LoyaltyAccount>()
            .FirstOrDefaultAsync(a => a.UserId == request.UserId, cancellationToken);

        if (account == null)
        {
            _logger.LogWarning("LoyaltyAccount not found. UserId: {UserId}", request.UserId);
            throw new NotFoundException("Sadakat hesabı", request.UserId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        try
        {
            account.DeductPoints(request.Points, $"Redeemed {request.Points} points");
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Insufficient points. UserId: {UserId}, Points: {Points}, Error: {Error}", 
                request.UserId, request.Points, ex.Message);
            throw new BusinessException(ex.Message);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var transaction = LoyaltyTransaction.Create(
            request.UserId,
            account.Id,
            -request.Points,
            LoyaltyTransactionType.Redeem,
            $"Redeemed {request.Points} points",
            // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
            DateTime.UtcNow.AddYears(_marketingSettings.PointsExpiryYears),
            request.OrderId,
            null);

        await _context.Set<LoyaltyTransaction>().AddAsync(transaction, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Points redeemed successfully. UserId: {UserId}, Points: {Points}", request.UserId, request.Points);

        return true;
    }
}
