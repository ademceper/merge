using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.CreateReferralCode;

public class CreateReferralCodeCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateReferralCodeCommandHandler> logger,
    IOptions<ReferralSettings> referralSettings,
    IOptions<MarketingSettings> marketingSettings) : IRequestHandler<CreateReferralCodeCommand, ReferralCodeDto>
{
    public async Task<ReferralCodeDto> Handle(CreateReferralCodeCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating referral code. UserId: {UserId}", request.UserId);

        var exists = await context.Set<ReferralCode>()
            .AsNoTracking()
            .AnyAsync(c => c.UserId == request.UserId, cancellationToken);

        if (exists)
        {
            logger.LogWarning("ReferralCode already exists. UserId: {UserId}", request.UserId);
            throw new BusinessException("Referans kodu zaten mevcut.");
        }

        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        
        var referralCode = GenerateCode(user?.Email ?? "USER");
        
        var newCode = ReferralCode.Create(
            request.UserId,
            referralCode,
            0, // MaxUsage = 0 (unlimited)
            null, // ExpiresAt = null (no expiration)
            referralSettings.Value.ReferrerPointsReward,
            (decimal)referralSettings.Value.RefereeDiscountPercentage);

        await context.Set<ReferralCode>().AddAsync(newCode, cancellationToken);
        
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var createdCode = await context.Set<ReferralCode>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == newCode.Id, cancellationToken);

        if (createdCode == null)
        {
            logger.LogWarning("ReferralCode not found after creation. UserId: {UserId}", request.UserId);
            throw new NotFoundException("Referans kodu", newCode.Id);
        }

        logger.LogInformation("ReferralCode created successfully. UserId: {UserId}, Code: {Code}", request.UserId, referralCode);

        return mapper.Map<ReferralCodeDto>(createdCode);
    }

    private string GenerateCode(string email)
    {
        var prefix = email.Split('@')[0].ToUpper().Substring(0, Math.Min(4, email.Length));
        var random = Random.Shared.Next(marketingSettings.Value.ReferralCodeMinRandom, marketingSettings.Value.ReferralCodeMaxRandom);
        return $"{prefix}{random}";
    }
}
