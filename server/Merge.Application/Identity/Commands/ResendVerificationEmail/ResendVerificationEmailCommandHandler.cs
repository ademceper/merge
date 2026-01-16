using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Application.Services.Notification;
using Merge.Domain.Entities;
using UserEntity = Merge.Domain.Modules.Identity.User;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Marketing.EmailVerification>;

namespace Merge.Application.Identity.Commands.ResendVerificationEmail;

public class ResendVerificationEmailCommandHandler(
    IRepository emailVerificationRepository,
    UserManager<UserEntity> userManager,
    IDbContext context,
    IUnitOfWork unitOfWork,
    IOptions<EmailSettings> emailSettings,
    ILogger<ResendVerificationEmailCommandHandler> logger,
    IEmailService? emailService = null) : IRequestHandler<ResendVerificationEmailCommand, Unit>
{

    public async Task<Unit> Handle(ResendVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Resend verification email attempt. UserId: {UserId}", request.UserId);

        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            logger.LogWarning("Resend verification email failed - user not found. UserId: {UserId}", request.UserId);
            throw new NotFoundException("Kullanıcı", request.UserId);
        }

        if (user.EmailConfirmed)
        {
            logger.LogInformation("Resend verification email skipped - email already verified. UserId: {UserId}", request.UserId);
            throw new BusinessException("Email zaten doğrulanmış.");
        }

        await GenerateVerificationTokenAsync(request.UserId, user.Email ?? string.Empty, cancellationToken);
        
        logger.LogInformation("Verification email resent successfully. UserId: {UserId}", request.UserId);
        return Unit.Value;
    }

    private async Task<string> GenerateVerificationTokenAsync(Guid userId, string email, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", userId);
        }

        var oldVerifications = await context.Set<EmailVerification>()
            .Where(ev => ev.UserId == userId && !ev.IsVerified)
            .OrderBy(ev => ev.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        context.Set<EmailVerification>().RemoveRange(oldVerifications);

        var token = Guid.NewGuid().ToString("N");
        var verification = EmailVerification.Create(
            userId,
            email,
            token,
            DateTime.UtcNow.AddHours(emailSettings.Value.VerificationTokenExpirationHours));

        await emailVerificationRepository.AddAsync(verification);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (emailService != null)
        {
            var verificationUrl = $"{emailSettings.Value.VerificationUrlPath}?token={token}";
            var emailBody = $@"
                <h2>Email Doğrulama</h2>
                <p>Hesabınızı doğrulamak için aşağıdaki linke tıklayın:</p>
                <p><a href=""{verificationUrl}"">Email'i Doğrula</a></p>
                <p>Bu link 24 saat geçerlidir.</p>
            ";
            await emailService.SendEmailAsync(email, "Email Doğrulama", emailBody);
        }

        return token;
    }
}

