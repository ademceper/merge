using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Identity.Commands.Verify2FACode;

public class Verify2FACodeCommandHandler(
    Merge.Application.Interfaces.IRepository<TwoFactorAuth> twoFactorRepository,
    Merge.Application.Interfaces.IRepository<TwoFactorCode> codeRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    IOptions<TwoFactorAuthSettings> twoFactorSettings,
    ILogger<Verify2FACodeCommandHandler> logger) : IRequestHandler<Verify2FACodeCommand, bool>
{

    public async Task<bool> Handle(Verify2FACodeCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Verifying 2FA code. UserId: {UserId}", request.UserId);

        var twoFactorAuth = await context.Set<TwoFactorAuth>()
            .FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken);

        if (twoFactorAuth == null || !twoFactorAuth.IsEnabled)
        {
            logger.LogWarning("2FA verification failed - not enabled. UserId: {UserId}", request.UserId);
            return false;
        }

        if (twoFactorAuth.LockedUntil.HasValue && twoFactorAuth.LockedUntil.Value > DateTime.UtcNow)
        {
            logger.LogWarning("2FA verification failed - account locked. UserId: {UserId}, LockedUntil: {LockedUntil}", 
                request.UserId, twoFactorAuth.LockedUntil.Value);
            throw new BusinessException($"Çok fazla başarısız deneme nedeniyle hesap kilitlendi. {twoFactorAuth.LockedUntil.Value.ToString(twoFactorSettings.Value.DateTimeFormat)} tarihinden sonra tekrar deneyin.");
        }

        bool isValid = false;

        if (twoFactorAuth.Method == TwoFactorMethod.Authenticator)
        {
            isValid = VerifyTOTP(twoFactorAuth.Secret, request.Code);
        }
        else
        {
            var twoFactorCode = await context.Set<TwoFactorCode>()
                .FirstOrDefaultAsync(c =>
                    c.UserId == request.UserId &&
                    c.Code == request.Code &&
                    !c.IsUsed &&
                    c.ExpiresAt > DateTime.UtcNow, cancellationToken);

            if (twoFactorCode != null)
            {
                twoFactorCode.MarkAsUsed();
                await codeRepository.UpdateAsync(twoFactorCode);
                isValid = true;
            }
        }

        if (!isValid)
        {
            twoFactorAuth.RecordFailedAttempt(
                twoFactorSettings.Value.MaxFailedAttempts,
                twoFactorSettings.Value.LockoutMinutes);
            await twoFactorRepository.UpdateAsync(twoFactorAuth);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            
            logger.LogWarning("2FA verification failed - invalid code. UserId: {UserId}, FailedAttempts: {FailedAttempts}", 
                request.UserId, twoFactorAuth.FailedAttempts);
            return false;
        }

        twoFactorAuth.ResetFailedAttempts();
        await twoFactorRepository.UpdateAsync(twoFactorAuth);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("2FA code verified successfully. UserId: {UserId}", request.UserId);
        return true;
    }

    private bool VerifyTOTP(string secret, string code)
    {
        try
        {
            var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var timeStep = unixTimestamp / twoFactorSettings.Value.TotpTimeStepSeconds;

            // Check current time step and one step before/after for clock skew
            for (long i = -1; i <= 1; i++)
            {
                var expectedCode = GenerateTOTP(secret, timeStep + i);
                if (expectedCode == code)
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "TOTP verification failed");
            return false;
        }
    }

    private string GenerateTOTP(string secret, long timeStep)
    {
        var keyBytes = Base32Decode(secret);
        var timeBytes = BitConverter.GetBytes(timeStep);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(timeBytes);
        }

        using var hmac = new HMACSHA1(keyBytes);


        var hash = hmac.ComputeHash(timeBytes);

        var offset = hash[hash.Length - 1] & 0x0F;
        var binary =
            ((hash[offset] & 0x7F) << 24) |
            ((hash[offset + 1] & 0xFF) << 16) |
            ((hash[offset + 2] & 0xFF) << 8) |
            (hash[offset + 3] & 0xFF);

        var otp = binary % 1000000;
        return otp.ToString("D6");
    }

    private byte[] Base32Decode(string encoded)
    {
        const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU) - Base32: her 5 bit = 1 byte, yaklaşık encoded.Length * 5 / 8
        // TOTP secret'ları genelde 16-32 karakter, bu yüzden maksimum 20 byte yeterli
        var result = new List<byte>(Math.Max(16, encoded.Length * 5 / 8));
        int buffer = 0;
        int bitsLeft = 0;

        foreach (char c in encoded.ToUpper())
        {
            int value = base32Chars.IndexOf(c);
            if (value < 0) continue;

            buffer = (buffer << 5) | value;
            bitsLeft += 5;

            if (bitsLeft >= 8)
            {
                result.Add((byte)(buffer >> (bitsLeft - 8)));
                bitsLeft -= 8;
            }
        }

        return result.ToArray();
    }
}

