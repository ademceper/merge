using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.SharedKernel.DomainEvents;
using UserEntity = Merge.Domain.Modules.Identity.User;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Marketing.EmailVerification>;

namespace Merge.Application.Identity.Commands.VerifyEmail;

public class VerifyEmailCommandHandler(
    IRepository emailVerificationRepository,
    IDbContext context,
    UserManager<UserEntity> userManager,
    IUnitOfWork unitOfWork,
    ILogger<VerifyEmailCommandHandler> logger) : IRequestHandler<VerifyEmailCommand, Unit>
{

    public async Task<Unit> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Email verification attempt. Token: {Token}", request.Token);

        var verification = await context.Set<EmailVerification>()
            .Include(ev => ev.User)
            .FirstOrDefaultAsync(ev => ev.Token == request.Token, cancellationToken);

        if (verification == null)
        {
            logger.LogWarning("Email verification failed - invalid token. Token: {Token}", request.Token);
            throw new BusinessException("Geçersiz token.");
        }

        if (verification.IsVerified)
        {
            logger.LogInformation("Email already verified. UserId: {UserId}", verification.UserId);
            return Unit.Value;
        }

        if (verification.ExpiresAt < DateTime.UtcNow)
        {
            logger.LogWarning("Email verification failed - token expired. Token: {Token}, ExpiresAt: {ExpiresAt}", request.Token, verification.ExpiresAt);
            throw new BusinessException("Doğrulama linki süresi dolmuş.");
        }

        verification.Verify();
        await emailVerificationRepository.UpdateAsync(verification);
        
        verification.User.ConfirmEmail();
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        await userManager.UpdateAsync(verification.User);

        logger.LogInformation("Email verified successfully. UserId: {UserId}, Email: {Email}", verification.UserId, verification.Email);
        return Unit.Value;
    }
}

