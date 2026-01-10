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

namespace Merge.Application.Marketing.Commands.AddPoints;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class AddPointsCommandHandler : IRequestHandler<AddPointsCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddPointsCommandHandler> _logger;
    private readonly MarketingSettings _marketingSettings;

    public AddPointsCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<AddPointsCommandHandler> logger,
        IOptions<MarketingSettings> marketingSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _marketingSettings = marketingSettings.Value;
    }

    public async Task<bool> Handle(AddPointsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding points. UserId: {UserId}, Points: {Points}, Type: {Type}, Description: {Description}",
            request.UserId, request.Points, request.Type, request.Description);

        var account = await _context.Set<LoyaltyAccount>()
            .FirstOrDefaultAsync(a => a.UserId == request.UserId, cancellationToken);

        if (account == null)
        {
            _logger.LogWarning("LoyaltyAccount not found. UserId: {UserId}", request.UserId);
            throw new NotFoundException("Sadakat hesabı", request.UserId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        account.AddPoints(request.Points, request.Description);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var transactionType = Enum.TryParse<LoyaltyTransactionType>(request.Type, true, out var type) 
            ? type 
            : LoyaltyTransactionType.Purchase;
        
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var expiresAt = DateTime.UtcNow.AddYears(_marketingSettings.PointsExpiryYears);
        var transaction = LoyaltyTransaction.Create(
            request.UserId,
            account.Id,
            request.Points,
            transactionType,
            request.Description,
            expiresAt,
            request.OrderId,
            null);

        await _context.Set<LoyaltyTransaction>().AddAsync(transaction, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Points added successfully. UserId: {UserId}, Points: {Points}, NewBalance: {NewBalance}",
            request.UserId, request.Points, account.PointsBalance);

        return true;
    }
}
