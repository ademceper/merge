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
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<TwoFactorAuth>;

namespace Merge.Application.Identity.Commands.Setup2FA;

public class Setup2FACommandHandler(
    IRepository twoFactorRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMediator mediator,
    IOptions<TwoFactorAuthSettings> twoFactorSettings,
    IOptions<JwtSettings> jwtSettings,
    ILogger<Setup2FACommandHandler> logger) : IRequestHandler<Setup2FACommand, TwoFactorSetupResponseDto>
{

    public async Task<TwoFactorSetupResponseDto> Handle(Setup2FACommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Setting up 2FA. UserId: {UserId}, Method: {Method}", request.UserId, request.SetupDto.Method);

        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            logger.LogWarning("2FA setup failed - user not found. UserId: {UserId}", request.UserId);
            throw new NotFoundException("Kullanıcı", request.UserId);
        }

        var existing2FA = await context.Set<TwoFactorAuth>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken);

        if (existing2FA != null && existing2FA.IsEnabled)
        {
            logger.LogWarning("2FA setup failed - already enabled. UserId: {UserId}", request.UserId);
            throw new BusinessException("2FA zaten etkin. Değiştirmek için önce devre dışı bırakın.");
        }

        var secret = GenerateTOTPSecret();
        var backupCodes = GenerateBackupCodes(twoFactorSettings.Value.BackupCodeCount);

        TwoFactorAuth twoFactorAuth;
        if (existing2FA == null)
        {
            twoFactorAuth = TwoFactorAuth.Create(
                request.UserId,
                request.SetupDto.Method,
                secret,
                request.SetupDto.PhoneNumber,
                request.SetupDto.Email ?? user.Email,
                backupCodes);
            await twoFactorRepository.AddAsync(twoFactorAuth);
        }
        else
        {
            twoFactorAuth = existing2FA;
            twoFactorAuth.UpdateSetup(
                request.SetupDto.Method,
                secret,
                request.SetupDto.PhoneNumber,
                request.SetupDto.Email ?? user.Email,
                backupCodes);
            await twoFactorRepository.UpdateAsync(twoFactorAuth);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new TwoFactorSetupResponseDto(
            Secret: string.Empty,
            QrCodeUrl: string.Empty,
            BackupCodes: backupCodes,
            Message: "2FA setup initiated. Please verify with a code to enable.");

        if (request.SetupDto.Method == TwoFactorMethod.Authenticator)
        {
            response = new TwoFactorSetupResponseDto(
                Secret: secret,
                QrCodeUrl: GenerateQRCodeUrl(secret, user.Email ?? string.Empty),
                BackupCodes: backupCodes,
                Message: "2FA setup initiated. Please verify with a code to enable.");
        }
        else
        {
            var sendCodeCommand = new SendVerificationCodeCommand(request.UserId, "Enable2FA");
            await mediator.Send(sendCodeCommand, cancellationToken);
            response = new TwoFactorSetupResponseDto(
                Secret: string.Empty,
                QrCodeUrl: string.Empty,
                BackupCodes: backupCodes,
                Message: $"Verification code sent via {request.SetupDto.Method}. Please verify to enable 2FA.");
        }

        logger.LogInformation("2FA setup completed successfully. UserId: {UserId}, Method: {Method}", request.UserId, request.SetupDto.Method);
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
        var issuer = jwtSettings.Value.Issuer;
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

