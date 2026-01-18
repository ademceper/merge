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

        var exists = await context.Set<LoyaltyAccount>()
            .AsNoTracking()
            .AnyAsync(a => a.UserId == request.UserId, cancellationToken);

        if (exists)
        {
            logger.LogWarning("Loyalty account already exists. UserId: {UserId}", request.UserId);
            throw new BusinessException("Sadakat hesabı zaten mevcut.");
        }

        var account = LoyaltyAccount.Create(request.UserId);

        await context.Set<LoyaltyAccount>().AddAsync(account, cancellationToken);
        
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Signup bonus points ekle
        account.AddPoints(_loyaltySettings.SignupBonusPoints, "Signup bonus");
        
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

        return mapper.Map<LoyaltyAccountDto>(createdAccount);
    }
}
