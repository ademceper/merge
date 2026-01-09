using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Identity;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Interfaces.User;
using Merge.Application.Identity.Commands.SendVerificationCode;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Identity.Commands.Setup2FA;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 12.1: Magic Number Sorunu - Configuration kullanımı
public class Setup2FACommandHandler : IRequestHandler<Setup2FACommand, TwoFactorSetupResponseDto>
{
    private readonly IRepository<TwoFactorAuth> _twoFactorRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly TwoFactorAuthSettings _twoFactorSettings;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<Setup2FACommandHandler> _logger;

    public Setup2FACommandHandler(
        IRepository<TwoFactorAuth> twoFactorRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IOptions<TwoFactorAuthSettings> twoFactorSettings,
        IOptions<JwtSettings> jwtSettings,
        ILogger<Setup2FACommandHandler> logger)
    {
        _twoFactorRepository = twoFactorRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _twoFactorSettings = twoFactorSettings.Value;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<TwoFactorSetupResponseDto> Handle(Setup2FACommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Setting up 2FA. UserId: {UserId}, Method: {Method}", request.UserId, request.SetupDto.Method);

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("2FA setup failed - user not found. UserId: {UserId}", request.UserId);
            throw new NotFoundException("Kullanıcı", request.UserId);
        }

        var existing2FA = await _context.Set<TwoFactorAuth>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken);

        if (existing2FA != null && existing2FA.IsEnabled)
        {
            _logger.LogWarning("2FA setup failed - already enabled. UserId: {UserId}", request.UserId);
            throw new BusinessException("2FA zaten etkin. Değiştirmek için önce devre dışı bırakın.");
        }

        var secret = GenerateTOTPSecret();
        // ✅ BOLUM 12.1: Magic Number Sorunu - Configuration kullanımı
        var backupCodes = GenerateBackupCodes(_twoFactorSettings.BackupCodeCount);

        TwoFactorAuth twoFactorAuth;
        if (existing2FA == null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            twoFactorAuth = TwoFactorAuth.Create(
                request.UserId,
                request.SetupDto.Method,
                secret,
                request.SetupDto.PhoneNumber,
                request.SetupDto.Email ?? user.Email,
                backupCodes);
            await _twoFactorRepository.AddAsync(twoFactorAuth);
        }
        else
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            twoFactorAuth = existing2FA;
            twoFactorAuth.UpdateSetup(
                request.SetupDto.Method,
                secret,
                request.SetupDto.PhoneNumber,
                request.SetupDto.Email ?? user.Email,
                backupCodes);
            await _twoFactorRepository.UpdateAsync(twoFactorAuth);
        }
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var response = new TwoFactorSetupResponseDto(
            Secret: string.Empty,
            QrCodeUrl: string.Empty,
            BackupCodes: backupCodes,
            Message: "2FA setup initiated. Please verify with a code to enable.");

        // For authenticator app method
        if (request.SetupDto.Method == TwoFactorMethod.Authenticator)
        {
            response = new TwoFactorSetupResponseDto(
                Secret: secret,
                QrCodeUrl: GenerateQRCodeUrl(secret, user.Email ?? string.Empty),
                BackupCodes: backupCodes,
                Message: "2FA setup initiated. Please verify with a code to enable.");
        }
        // For SMS/Email methods, send verification code
        else
        {
            var sendCodeCommand = new SendVerificationCodeCommand(request.UserId, "Enable2FA");
            await _mediator.Send(sendCodeCommand, cancellationToken);
            response = new TwoFactorSetupResponseDto(
                Secret: string.Empty,
                QrCodeUrl: string.Empty,
                BackupCodes: backupCodes,
                Message: $"Verification code sent via {request.SetupDto.Method}. Please verify to enable 2FA.");
        }

        _logger.LogInformation("2FA setup completed successfully. UserId: {UserId}, Method: {Method}", request.UserId, request.SetupDto.Method);
        return response;
    }

    private string GenerateTOTPSecret()
    {
        var bytes = new byte[20];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Base32Encode(bytes);
    }

    private string GenerateQRCodeUrl(string secret, string email)
    {
        // ✅ BOLUM 12.1: Magic Number Sorunu - Configuration kullanımı
        var issuer = _jwtSettings.Issuer;
        var otpauthUrl = $"otpauth://totp/{issuer}:{email}?secret={secret}&issuer={issuer}";
        return otpauthUrl;
    }

    private string[] GenerateBackupCodes(int count)
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
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        var code = BitConverter.ToString(bytes).Replace("-", "");
        // ✅ PERFORMANCE: Span<char> kullanımı (zero allocation)
        var codeSpan = code.AsSpan();
        return $"{codeSpan[..4]}-{codeSpan[4..8]}";
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

    private string Base32Encode(byte[] data)
    {
        const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var result = new System.Text.StringBuilder();
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
}

