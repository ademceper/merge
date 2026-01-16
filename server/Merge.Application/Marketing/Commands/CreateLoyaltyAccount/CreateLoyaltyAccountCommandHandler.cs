using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.CreateLoyaltyAccount;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class CreateLoyaltyAccountCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateLoyaltyAccountCommandHandler> logger,
    IOptions<LoyaltySettings> loyaltySettings) : IRequestHandler<CreateLoyaltyAccountCommand, LoyaltyAccountDto>
{
    private readonly LoyaltySettings _loyaltySettings = loyaltySettings.Value;

    public async Task<LoyaltyAccountDto> Handle(CreateLoyaltyAccountCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating loyalty account. UserId: {UserId}", request.UserId);

        // ✅ PERFORMANCE: AsNoTracking - Check if account already exists
        var exists = await context.Set<LoyaltyAccount>()
            .AsNoTracking()
            .AnyAsync(a => a.UserId == request.UserId, cancellationToken);

        if (exists)
        {
            logger.LogWarning("Loyalty account already exists. UserId: {UserId}", request.UserId);
            throw new BusinessException("Sadakat hesabı zaten mevcut.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var account = LoyaltyAccount.Create(request.UserId);

        await context.Set<LoyaltyAccount>().AddAsync(account, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan
        // Signup bonus points ekle
        account.AddPoints(_loyaltySettings.SignupBonusPoints, "Signup bonus");
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var createdAccount = await context.Set<LoyaltyAccount>()
            .AsNoTracking()
            .Include(a => a.Tier)
            .FirstOrDefaultAsync(a => a.Id == account.Id, cancellationToken);

        if (createdAccount == null)
        {
            logger.LogWarning("LoyaltyAccount not found after creation. AccountId: {AccountId}", account.Id);
            throw new NotFoundException("Sadakat hesabı", account.Id);
        }

        logger.LogInformation("LoyaltyAccount created successfully. AccountId: {AccountId}, UserId: {UserId}, SignupBonusPoints: {SignupBonusPoints}", 
            account.Id, request.UserId, _loyaltySettings.SignupBonusPoints);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<LoyaltyAccountDto>(createdAccount);
    }
}
