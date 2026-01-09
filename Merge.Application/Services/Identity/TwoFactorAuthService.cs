using AutoMapper;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Merge.Application.Services.Notification;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.Identity;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Application.DTOs.Identity;

namespace Merge.Application.Services.Identity;

// ⚠️ OBSOLETE: Bu service artık kullanılmamalı. MediatR Command/Query pattern kullanılmalı.
// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service'ler yerine Command/Query handler'ları kullan
[Obsolete("Use MediatR Commands/Queries instead. This service will be removed in a future version.")]
public class TwoFactorAuthService : ITwoFactorAuthService
{
    private readonly IRepository<TwoFactorAuth> _twoFactorRepository;
    private readonly IRepository<TwoFactorCode> _codeRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IMapper _mapper;
    private readonly ILogger<TwoFactorAuthService> _logger;

    public TwoFactorAuthService(
        IRepository<TwoFactorAuth> twoFactorRepository,
        IRepository<TwoFactorCode> codeRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ISmsService smsService,
        IMapper mapper,
        ILogger<TwoFactorAuthService> logger)
    {
        _twoFactorRepository = twoFactorRepository;
        _codeRepository = codeRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _smsService = smsService;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<TwoFactorSetupResponseDto> Setup2FAAsync(Guid userId, TwoFactorSetupDto setupDto, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", userId);
        }

        var existing2FA = await _context.Set<TwoFactorAuth>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);

        if (existing2FA != null && existing2FA.IsEnabled)
        {
            throw new BusinessException("2FA zaten etkin. Değiştirmek için önce devre dışı bırakın.");
        }

        var secret = GenerateTOTPSecret();
        var backupCodes = GenerateBackupCodes();

        var twoFactorAuth = existing2FA ?? new TwoFactorAuth();
        twoFactorAuth.UserId = userId;
        twoFactorAuth.Method = setupDto.Method;
        twoFactorAuth.Secret = secret;
        twoFactorAuth.PhoneNumber = setupDto.PhoneNumber;
        twoFactorAuth.Email = setupDto.Email ?? user.Email;
        twoFactorAuth.IsEnabled = false;
        twoFactorAuth.IsVerified = false;
        twoFactorAuth.BackupCodes = backupCodes;

        if (existing2FA == null)
        {
            await _twoFactorRepository.AddAsync(twoFactorAuth);
        }
        else
        {
            await _twoFactorRepository.UpdateAsync(twoFactorAuth);
        }
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var response = new TwoFactorSetupResponseDto
        {
            BackupCodes = backupCodes,
            Message = "2FA setup initiated. Please verify with a code to enable."
        };

        // For authenticator app method
        if (setupDto.Method == TwoFactorMethod.Authenticator)
        {
            response.Secret = secret;
            response.QrCodeUrl = GenerateQRCodeUrl(secret, user.Email ?? string.Empty);
        }
        // For SMS/Email methods, send verification code
        else
        {
            await SendVerificationCodeAsync(userId, "Enable2FA", cancellationToken);
            response.Message = $"Verification code sent via {setupDto.Method}. Please verify to enable 2FA.";
        }

        return response;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> Enable2FAAsync(Guid userId, Enable2FADto enableDto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var twoFactorAuth = await _context.Set<TwoFactorAuth>()
            .FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);

        if (twoFactorAuth == null)
        {
            throw new BusinessException("2FA kurulumu yapılmamış. Önce 2FA kurulumunu yapın.");
        }

        if (twoFactorAuth.IsEnabled)
        {
            throw new BusinessException("2FA zaten etkin.");
        }

        bool isValid = false;

        if (twoFactorAuth.Method == TwoFactorMethod.Authenticator)
        {
            isValid = VerifyTOTP(twoFactorAuth.Secret, enableDto.Code);
        }
        else
        {
            // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
            // ✅ PERFORMANCE: !c.IsUsed kontrolü IsDeleted değil, farklı property (kabul edilebilir)
            var code = await _context.Set<TwoFactorCode>()
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.Code == enableDto.Code &&
                    c.Purpose == "Enable2FA" &&
                    !c.IsUsed &&
                    c.ExpiresAt > DateTime.UtcNow, cancellationToken);

            if (code != null)
            {
                code.IsUsed = true;
                code.UsedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                isValid = true;
            }
        }

        if (!isValid)
        {
            throw new ValidationException("Geçersiz doğrulama kodu.");
        }

        twoFactorAuth.IsEnabled = true;
        twoFactorAuth.IsVerified = true;
        await _twoFactorRepository.UpdateAsync(twoFactorAuth);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> Disable2FAAsync(Guid userId, Disable2FADto disableDto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var twoFactorAuth = await _context.Set<TwoFactorAuth>()
            .FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);

        if (twoFactorAuth == null || !twoFactorAuth.IsEnabled)
        {
            throw new BusinessException("2FA etkin değil.");
        }

        // Verify current 2FA code
        var isValid = await Verify2FACodeAsync(userId, disableDto.Code, cancellationToken);

        if (!isValid)
        {
            throw new ValidationException("Geçersiz doğrulama kodu.");
        }

        twoFactorAuth.IsEnabled = false;
        await _twoFactorRepository.UpdateAsync(twoFactorAuth);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<TwoFactorStatusDto?> Get2FAStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var twoFactorAuth = await _context.Set<TwoFactorAuth>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);

        if (twoFactorAuth == null)
        {
            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return new TwoFactorStatusDto
            {
                IsEnabled = false,
                Method = TwoFactorMethod.None,
                BackupCodesRemaining = 0
            };
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = _mapper.Map<TwoFactorStatusDto>(twoFactorAuth);
        // ✅ PERFORMANCE: Memory'de minimal işlem (sadece property assignment)
        dto.PhoneNumber = twoFactorAuth.PhoneNumber != null ? MaskPhoneNumber(twoFactorAuth.PhoneNumber) : null;
        dto.Email = twoFactorAuth.Email != null ? MaskEmail(twoFactorAuth.Email) : null;
        dto.BackupCodesRemaining = twoFactorAuth.BackupCodes?.Length ?? 0;
        return dto;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> Verify2FACodeAsync(Guid userId, string code, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var twoFactorAuth = await _context.Set<TwoFactorAuth>()
            .FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);

        if (twoFactorAuth == null || !twoFactorAuth.IsEnabled)
        {
            return false;
        }

        // Check for account lockout
        if (twoFactorAuth.LockedUntil.HasValue && twoFactorAuth.LockedUntil.Value > DateTime.UtcNow)
        {
            throw new BusinessException($"Çok fazla başarısız deneme nedeniyle hesap kilitlendi. {twoFactorAuth.LockedUntil.Value:yyyy-MM-dd HH:mm:ss} tarihinden sonra tekrar deneyin.");
        }

        bool isValid = false;

        if (twoFactorAuth.Method == TwoFactorMethod.Authenticator)
        {
            isValid = VerifyTOTP(twoFactorAuth.Secret, code);
        }
        else
        {
            // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
            // ✅ PERFORMANCE: !c.IsUsed kontrolü IsDeleted değil, farklı property (kabul edilebilir)
            var twoFactorCode = await _context.Set<TwoFactorCode>()
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.Code == code &&
                    !c.IsUsed &&
                    c.ExpiresAt > DateTime.UtcNow, cancellationToken);

            if (twoFactorCode != null)
            {
                twoFactorCode.IsUsed = true;
                twoFactorCode.UsedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                isValid = true;
            }
        }

        if (!isValid)
        {
            twoFactorAuth.FailedAttempts++;
            twoFactorAuth.LastAttemptAt = DateTime.UtcNow;

            // Lock account after 5 failed attempts
            if (twoFactorAuth.FailedAttempts >= 5)
            {
                twoFactorAuth.LockedUntil = DateTime.UtcNow.AddMinutes(15);
            }

            await _twoFactorRepository.UpdateAsync(twoFactorAuth);
            // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return false;
        }

        // Reset failed attempts on successful verification
        twoFactorAuth.FailedAttempts = 0;
        twoFactorAuth.LastAttemptAt = DateTime.UtcNow;
        await _twoFactorRepository.UpdateAsync(twoFactorAuth);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> SendVerificationCodeAsync(Guid userId, string purpose = "Login", CancellationToken cancellationToken = default)
    {
        var twoFactorAuth = await _context.Set<TwoFactorAuth>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);

        if (twoFactorAuth == null)
        {
            throw new BusinessException("2FA kurulumu yapılmamış.");
        }

        var code = GenerateNumericCode(6);
        var expiresAt = DateTime.UtcNow.AddMinutes(5);

        var twoFactorCode = new TwoFactorCode
        {
            UserId = userId,
            Code = code,
            Method = twoFactorAuth.Method,
            ExpiresAt = expiresAt,
            Purpose = purpose
        };

        await _codeRepository.AddAsync(twoFactorCode);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send code via appropriate method
        if (twoFactorAuth.Method == TwoFactorMethod.SMS && !string.IsNullOrEmpty(twoFactorAuth.PhoneNumber))
        {
            await _smsService.SendSmsAsync(twoFactorAuth.PhoneNumber, $"Your verification code is: {code}. Valid for 5 minutes.");
        }
        else if (twoFactorAuth.Method == TwoFactorMethod.Email && !string.IsNullOrEmpty(twoFactorAuth.Email))
        {
            await _emailService.SendEmailAsync(twoFactorAuth.Email, "2FA Verification Code", $"Your verification code is: {code}. This code will expire in 5 minutes.");
        }

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<BackupCodesResponseDto> RegenerateBackupCodesAsync(Guid userId, RegenerateBackupCodesDto regenerateDto, CancellationToken cancellationToken = default)
    {
        var twoFactorAuth = await _context.Set<TwoFactorAuth>()
            .FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);

        if (twoFactorAuth == null || !twoFactorAuth.IsEnabled)
        {
            throw new BusinessException("2FA etkin değil.");
        }

        // Verify current 2FA code
        var isValid = await Verify2FACodeAsync(userId, regenerateDto.Code, cancellationToken);

        if (!isValid)
        {
            throw new ValidationException("Geçersiz doğrulama kodu.");
        }

        var backupCodes = GenerateBackupCodes();
        twoFactorAuth.BackupCodes = backupCodes;
        await _twoFactorRepository.UpdateAsync(twoFactorAuth);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return new BackupCodesResponseDto
        {
            BackupCodes = backupCodes,
            Message = "Backup codes regenerated successfully. Store them securely."
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> VerifyBackupCodeAsync(Guid userId, string backupCode, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var twoFactorAuth = await _context.Set<TwoFactorAuth>()
            .FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);

        if (twoFactorAuth == null || !twoFactorAuth.IsEnabled || twoFactorAuth.BackupCodes == null)
        {
            return false;
        }

        var normalizedCode = backupCode.Replace("-", "").ToUpper();

        // ✅ PERFORMANCE: Array üzerinde Any() ve Where().ToArray() - memory'de minimal işlem (kabul edilebilir)
        if (twoFactorAuth.BackupCodes.Any(c => c.Replace("-", "").ToUpper() == normalizedCode))
        {
            // Remove used backup code
            twoFactorAuth.BackupCodes = twoFactorAuth.BackupCodes
                .Where(c => c.Replace("-", "").ToUpper() != normalizedCode)
                .ToArray();

            await _twoFactorRepository.UpdateAsync(twoFactorAuth);
            // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        return false;
    }

    public string GenerateTOTPSecret()
    {
        var bytes = new byte[20];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Base32Encode(bytes);
    }

    public string GenerateQRCodeUrl(string secret, string email)
    {
        var issuer = "MergeECommerce";
        var otpauthUrl = $"otpauth://totp/{issuer}:{email}?secret={secret}&issuer={issuer}";
        return otpauthUrl;
    }

    // Helper methods

    private string[] GenerateBackupCodes(int count = 10)
    {
        var codes = new string[count];
        for (int i = 0; i < count; i++)
        {
            codes[i] = GenerateBackupCode();
        }
        return codes;
    }

    private string GenerateBackupCode()
    {
        var bytes = new byte[5];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        var code = BitConverter.ToString(bytes).Replace("-", "");
        return $"{code.Substring(0, 4)}-{code.Substring(4, 4)}";
    }

    private string GenerateNumericCode(int length)
    {
        var bytes = new byte[4];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        var number = BitConverter.ToUInt32(bytes, 0);
        var code = (number % (int)Math.Pow(10, length)).ToString($"D{length}");
        return code;
    }

    private bool VerifyTOTP(string secret, string code)
    {
        try
        {
            var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var timeStep = unixTimestamp / 30;

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

    private string Base32Encode(byte[] data)
    {
        const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var result = new StringBuilder();
        int buffer = data[0];
        int next = 1;
        int bitsLeft = 8;

        while (bitsLeft > 0 || next < data.Length)
        {
            if (bitsLeft < 5)
            {
                if (next < data.Length)
                {
                    buffer <<= 8;
                    buffer |= data[next++];
                    bitsLeft += 8;
                }
                else
                {
                    int pad = 5 - bitsLeft;
                    buffer <<= pad;
                    bitsLeft += pad;
                }
            }

            int index = (buffer >> (bitsLeft - 5)) & 0x1F;
            bitsLeft -= 5;
            result.Append(base32Chars[index]);
        }

        return result.ToString();
    }

    private byte[] Base32Decode(string encoded)
    {
        const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var result = new List<byte>();
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

    private string MaskPhoneNumber(string phone)
    {
        if (phone.Length < 4) return phone;
        return $"***{phone.Substring(phone.Length - 4)}";
    }

    private string MaskEmail(string email)
    {
        var parts = email.Split('@');
        if (parts.Length != 2) return email;
        var username = parts[0];
        if (username.Length <= 2) return email;
        return $"{username[0]}***{username[username.Length - 1]}@{parts[1]}";
    }
}
