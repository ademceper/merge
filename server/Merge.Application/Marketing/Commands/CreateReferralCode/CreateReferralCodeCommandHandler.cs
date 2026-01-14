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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
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

        // ✅ PERFORMANCE: AsNoTracking - Check if code already exists
        var exists = await context.Set<ReferralCode>()
            .AsNoTracking()
            .AnyAsync(c => c.UserId == request.UserId, cancellationToken);

        if (exists)
        {
            logger.LogWarning("ReferralCode already exists. UserId: {UserId}", request.UserId);
            throw new BusinessException("Referans kodu zaten mevcut.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
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
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking ile tek query'de getir
        var createdCode = await context.Set<ReferralCode>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == newCode.Id, cancellationToken);

        if (createdCode == null)
        {
            logger.LogWarning("ReferralCode not found after creation. UserId: {UserId}", request.UserId);
            throw new NotFoundException("Referans kodu", newCode.Id);
        }

        logger.LogInformation("ReferralCode created successfully. UserId: {UserId}, Code: {Code}", request.UserId, referralCode);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<ReferralCodeDto>(createdCode);
    }

    private string GenerateCode(string email)
    {
        var prefix = email.Split('@')[0].ToUpper().Substring(0, Math.Min(4, email.Length));
        // ✅ THREAD SAFETY: Random.Shared kullan (new Random() thread-safe değil)
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var random = Random.Shared.Next(marketingSettings.Value.ReferralCodeMinRandom, marketingSettings.Value.ReferralCodeMaxRandom);
        return $"{prefix}{random}";
    }
}
