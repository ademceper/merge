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

namespace Merge.Application.Identity.Commands.Verify2FACode;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 12.1: Magic Number Sorunu - Configuration kullanımı
public class Verify2FACodeCommandHandler : IRequestHandler<Verify2FACodeCommand, bool>
{
    private readonly IRepository<TwoFactorAuth> _twoFactorRepository;
    private readonly IRepository<TwoFactorCode> _codeRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TwoFactorAuthSettings _twoFactorSettings;
    private readonly ILogger<Verify2FACodeCommandHandler> _logger;

    public Verify2FACodeCommandHandler(
        IRepository<TwoFactorAuth> twoFactorRepository,
        IRepository<TwoFactorCode> codeRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        IOptions<TwoFactorAuthSettings> twoFactorSettings,
        ILogger<Verify2FACodeCommandHandler> logger)
    {
        _twoFactorRepository = twoFactorRepository;
        _codeRepository = codeRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _twoFactorSettings = twoFactorSettings.Value;
        _logger = logger;
    }

    public async Task<bool> Handle(Verify2FACodeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Verifying 2FA code. UserId: {UserId}", request.UserId);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var twoFactorAuth = await _context.Set<TwoFactorAuth>()
            .FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken);

        if (twoFactorAuth == null || !twoFactorAuth.IsEnabled)
        {
            _logger.LogWarning("2FA verification failed - not enabled. UserId: {UserId}", request.UserId);
            return false;
        }

        // Check for account lockout
        if (twoFactorAuth.LockedUntil.HasValue && twoFactorAuth.LockedUntil.Value > DateTime.UtcNow)
        {
            _logger.LogWarning("2FA verification failed - account locked. UserId: {UserId}, LockedUntil: {LockedUntil}", 
                request.UserId, twoFactorAuth.LockedUntil.Value);
            // ✅ BOLUM 12.1: Magic Number Sorunu - Configuration kullanımı
            throw new BusinessException($"Çok fazla başarısız deneme nedeniyle hesap kilitlendi. {twoFactorAuth.LockedUntil.Value.ToString(_twoFactorSettings.DateTimeFormat)} tarihinden sonra tekrar deneyin.");
        }

        bool isValid = false;

        if (twoFactorAuth.Method == TwoFactorMethod.Authenticator)
        {
            isValid = VerifyTOTP(twoFactorAuth.Secret, request.Code);
        }
        else
        {
            // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
            var twoFactorCode = await _context.Set<TwoFactorCode>()
                .FirstOrDefaultAsync(c =>
                    c.UserId == request.UserId &&
                    c.Code == request.Code &&
                    !c.IsUsed &&
                    c.ExpiresAt > DateTime.UtcNow, cancellationToken);

            if (twoFactorCode != null)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                twoFactorCode.MarkAsUsed();
                await _codeRepository.UpdateAsync(twoFactorCode);
                isValid = true;
            }
        }

        if (!isValid)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            // ✅ BOLUM 12.1: Magic Number Sorunu - Configuration kullanımı
            twoFactorAuth.RecordFailedAttempt(
                _twoFactorSettings.MaxFailedAttempts,
                _twoFactorSettings.LockoutMinutes);
            await _twoFactorRepository.UpdateAsync(twoFactorAuth);
            // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogWarning("2FA verification failed - invalid code. UserId: {UserId}, FailedAttempts: {FailedAttempts}", 
                request.UserId, twoFactorAuth.FailedAttempts);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        // Reset failed attempts on successful verification
        twoFactorAuth.ResetFailedAttempts();
        await _twoFactorRepository.UpdateAsync(twoFactorAuth);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("2FA code verified successfully. UserId: {UserId}", request.UserId);
        return true;
    }

    private bool VerifyTOTP(string secret, string code)
    {
        try
        {
            var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var timeStep = unixTimestamp / _twoFactorSettings.TotpTimeStepSeconds;

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
            _logger.LogWarning(ex, "TOTP verification failed");
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

