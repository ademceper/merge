using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Application.Services.Notification;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Identity.TwoFactorCode>;

namespace Merge.Application.Identity.Commands.SendVerificationCode;

public class SendVerificationCodeCommandHandler(
    IRepository codeRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    IOptions<TwoFactorAuthSettings> twoFactorSettings,
    ILogger<SendVerificationCodeCommandHandler> logger,
    IEmailService? emailService = null,
    ISmsService? smsService = null) : IRequestHandler<SendVerificationCodeCommand, Unit>
{

    public async Task<Unit> Handle(SendVerificationCodeCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending verification code. UserId: {UserId}, Purpose: {Purpose}", request.UserId, request.Purpose);

        var twoFactorAuth = await context.Set<TwoFactorAuth>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken);

        if (twoFactorAuth == null)
        {
            logger.LogWarning("Send verification code failed - 2FA setup not found. UserId: {UserId}", request.UserId);
            throw new BusinessException("2FA kurulumu yapılmamış.");
        }

        var code = GenerateNumericCode(twoFactorSettings.Value.VerificationCodeLength);
        var expiresAt = DateTime.UtcNow.AddMinutes(twoFactorSettings.Value.VerificationCodeExpirationMinutes);

        if (!Enum.TryParse<TwoFactorPurpose>(request.Purpose, true, out var purpose))
        {
            logger.LogWarning("Invalid TwoFactorPurpose: {Purpose}, defaulting to Login", request.Purpose);
            purpose = TwoFactorPurpose.Login;
        }

        var twoFactorCode = TwoFactorCode.Create(
            request.UserId,
            code,
            twoFactorAuth.Method,
            expiresAt,
            purpose);

        await codeRepository.AddAsync(twoFactorCode);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (twoFactorAuth.Method == TwoFactorMethod.SMS && !string.IsNullOrEmpty(twoFactorAuth.PhoneNumber))
        {
            if (smsService == null)
            {
                logger.LogError("SMS service not configured for sending 2FA code. UserId: {UserId}", request.UserId);
                throw new InvalidOperationException("SMS servisi yapılandırılmamış.");
            }
            await smsService.SendSmsAsync(twoFactorAuth.PhoneNumber, $"Your verification code is: {code}. Valid for {twoFactorSettings.Value.VerificationCodeExpirationMinutes} minutes.");
        }
        else if (twoFactorAuth.Method == TwoFactorMethod.Email && !string.IsNullOrEmpty(twoFactorAuth.Email))
        {
            if (emailService == null)
            {
                logger.LogError("Email service not configured for sending 2FA code. UserId: {UserId}", request.UserId);
                throw new InvalidOperationException("Email servisi yapılandırılmamış.");
            }
            await emailService.SendEmailAsync(twoFactorAuth.Email, "2FA Verification Code", $"Your verification code is: {code}. This code will expire in {twoFactorSettings.Value.VerificationCodeExpirationMinutes} minutes.");
        }
        else
        {
            logger.LogWarning("No valid method to send 2FA code. UserId: {UserId}, Method: {Method}", request.UserId, twoFactorAuth.Method);
            throw new BusinessException("Doğrulama kodu gönderilemedi. Geçerli bir yöntem yapılandırılmamış.");
        }

        logger.LogInformation("Verification code sent successfully. UserId: {UserId}, Purpose: {Purpose}", request.UserId, request.Purpose);
        return Unit.Value;
    }

    private string GenerateNumericCode(int length)
    {
        var bytes = new byte[4];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())

        {
            rng.GetBytes(bytes);
        }
        var number = BitConverter.ToUInt32(bytes, 0);
        var code = (number % (int)Math.Pow(10, length)).ToString($"D{length}");
        return code;
    }
}

