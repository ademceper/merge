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
public class CreateReferralCodeCommandHandler : IRequestHandler<CreateReferralCodeCommand, ReferralCodeDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateReferralCodeCommandHandler> _logger;
    private readonly ReferralSettings _referralSettings;
    private readonly MarketingSettings _marketingSettings;

    public CreateReferralCodeCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateReferralCodeCommandHandler> logger,
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

    public async Task<ReferralCodeDto> Handle(CreateReferralCodeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating referral code. UserId: {UserId}", request.UserId);

        // ✅ PERFORMANCE: AsNoTracking - Check if code already exists
        var exists = await _context.Set<ReferralCode>()
            .AsNoTracking()
            .AnyAsync(c => c.UserId == request.UserId, cancellationToken);

        if (exists)
        {
            _logger.LogWarning("ReferralCode already exists. UserId: {UserId}", request.UserId);
            throw new BusinessException("Referans kodu zaten mevcut.");
        }

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
        var createdCode = await _context.Set<ReferralCode>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == newCode.Id, cancellationToken);

        if (createdCode == null)
        {
            _logger.LogWarning("ReferralCode not found after creation. UserId: {UserId}", request.UserId);
            throw new NotFoundException("Referans kodu", newCode.Id);
        }

        _logger.LogInformation("ReferralCode created successfully. UserId: {UserId}, Code: {Code}", request.UserId, referralCode);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ReferralCodeDto>(createdCode);
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
