using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.ApplyReferralCode;

public class ApplyReferralCodeCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<ApplyReferralCodeCommandHandler> logger) : IRequestHandler<ApplyReferralCodeCommand, bool>
{
    public async Task<bool> Handle(ApplyReferralCodeCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Applying referral code. Code: {Code}, NewUserId: {NewUserId}", request.Code, request.NewUserId);

        var referralCode = await context.Set<ReferralCode>()
            .FirstOrDefaultAsync(c => c.Code == request.Code && c.IsActive, cancellationToken);

        if (referralCode is null)
        {
            logger.LogWarning("ReferralCode not found or inactive. Code: {Code}", request.Code);
            return false;
        }

        if (referralCode.UserId == request.NewUserId)
        {
            logger.LogWarning("User cannot refer themselves. Code: {Code}, UserId: {NewUserId}", request.Code, request.NewUserId);
            return false;
        }

        var exists = await context.Set<Referral>()
            .AsNoTracking()
            .AnyAsync(r => r.ReferredUserId == request.NewUserId, cancellationToken);

        if (exists)
        {
            logger.LogWarning("User already has a referral. NewUserId: {NewUserId}", request.NewUserId);
            return false;
        }

        referralCode.IncrementUsage();

        var referral = Referral.Create(
            referralCode.UserId,
            request.NewUserId,
            referralCode.Id,
            request.Code);

        await context.Set<Referral>().AddAsync(referral, cancellationToken);
        
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Referral code applied successfully. Code: {Code}, NewUserId: {NewUserId}", request.Code, request.NewUserId);

        return true;
    }
}
