using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Marketing.Commands.ApplyReferralCode;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ApplyReferralCodeCommandHandler : IRequestHandler<ApplyReferralCodeCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApplyReferralCodeCommandHandler> _logger;

    public ApplyReferralCodeCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ApplyReferralCodeCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ApplyReferralCodeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Applying referral code. Code: {Code}, NewUserId: {NewUserId}", request.Code, request.NewUserId);

        var referralCode = await _context.Set<ReferralCode>()
            .FirstOrDefaultAsync(c => c.Code == request.Code && c.IsActive, cancellationToken);

        if (referralCode == null)
        {
            _logger.LogWarning("ReferralCode not found or inactive. Code: {Code}", request.Code);
            return false;
        }

        if (referralCode.UserId == request.NewUserId)
        {
            _logger.LogWarning("User cannot refer themselves. Code: {Code}, UserId: {NewUserId}", request.Code, request.NewUserId);
            return false;
        }

        var exists = await _context.Set<Referral>()
            .AsNoTracking()
            .AnyAsync(r => r.ReferredUserId == request.NewUserId, cancellationToken);

        if (exists)
        {
            _logger.LogWarning("User already has a referral. NewUserId: {NewUserId}", request.NewUserId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        referralCode.IncrementUsage();

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var referral = Referral.Create(
            referralCode.UserId,
            request.NewUserId,
            referralCode.Id,
            request.Code);

        await _context.Set<Referral>().AddAsync(referral, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Referral code applied successfully. Code: {Code}, NewUserId: {NewUserId}", request.Code, request.NewUserId);

        return true;
    }
}
