using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;

namespace Merge.Application.Marketing.Queries.GetMyReferralCode;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetMyReferralCodeQueryHandler : IRequestHandler<GetMyReferralCodeQuery, ReferralCodeDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMyReferralCodeQueryHandler> _logger;
    private readonly ReferralSettings _referralSettings;
    private readonly MarketingSettings _marketingSettings;

    public GetMyReferralCodeQueryHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetMyReferralCodeQueryHandler> logger,
        IOptions<ReferralSettings> referralSettings,
        IOptions<MarketingSettings> marketingSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _referralSettings = referralSettings.Value;
        _marketingSettings = marketingSettings.Value;
    }

    public async Task<ReferralCodeDto> Handle(GetMyReferralCodeQuery request, CancellationToken cancellationToken)
    {
        var code = await _context.Set<ReferralCode>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

        if (code == null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            
            var referralCode = GenerateCode(user?.Email ?? "USER");
            
            var newCode = ReferralCode.Create(
                request.UserId,
                referralCode,
                0, // MaxUsage = 0 (unlimited)
                null, // ExpiresAt = null (no expiration)
                _referralSettings.ReferrerPointsReward,
                (decimal)_referralSettings.RefereeDiscountPercentage);

            await _context.Set<ReferralCode>().AddAsync(newCode, cancellationToken);
            
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
            // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ PERFORMANCE: AsNoTracking ile tek query'de getir
            code = await _context.Set<ReferralCode>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == newCode.Id, cancellationToken);

            if (code == null)
            {
                _logger.LogWarning("ReferralCode not found after creation. UserId: {UserId}", request.UserId);
                throw new Application.Exceptions.NotFoundException("Referans kodu", newCode.Id);
            }
        }

        return _mapper.Map<ReferralCodeDto>(code);
    }

    private string GenerateCode(string email)
    {
        var prefix = email.Split('@')[0].ToUpper().Substring(0, Math.Min(4, email.Length));
        // ✅ THREAD SAFETY: Random.Shared kullan (new Random() thread-safe değil)
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var random = Random.Shared.Next(_marketingSettings.ReferralCodeMinRandom, _marketingSettings.ReferralCodeMaxRandom);
        return $"{prefix}{random}";
    }
}
