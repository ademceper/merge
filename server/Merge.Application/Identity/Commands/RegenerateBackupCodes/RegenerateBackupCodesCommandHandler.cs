using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Identity;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Identity.Commands.Verify2FACode;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Identity.TwoFactorAuth>;

namespace Merge.Application.Identity.Commands.RegenerateBackupCodes;

public class RegenerateBackupCodesCommandHandler(
    IRepository twoFactorRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMediator mediator,
    IOptions<TwoFactorAuthSettings> twoFactorSettings,
    ILogger<RegenerateBackupCodesCommandHandler> logger) : IRequestHandler<RegenerateBackupCodesCommand, BackupCodesResponseDto>
{

    public async Task<BackupCodesResponseDto> Handle(RegenerateBackupCodesCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Regenerating backup codes. UserId: {UserId}", request.UserId);

        var twoFactorAuth = await context.Set<TwoFactorAuth>()
            .FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken);

        if (twoFactorAuth == null || !twoFactorAuth.IsEnabled)
        {
            logger.LogWarning("Regenerate backup codes failed - 2FA not enabled. UserId: {UserId}", request.UserId);
            throw new BusinessException("2FA etkin değil.");
        }

        var verifyCommand = new Verify2FACodeCommand(request.UserId, request.RegenerateDto.Code);
        var isValid = await mediator.Send(verifyCommand, cancellationToken);

        if (!isValid)
        {
            logger.LogWarning("Regenerate backup codes failed - invalid code. UserId: {UserId}", request.UserId);
            throw new ValidationException("Geçersiz doğrulama kodu.");
        }

        var backupCodes = GenerateBackupCodes(twoFactorSettings.Value.BackupCodeCount);
        twoFactorAuth.UpdateBackupCodes(backupCodes);
        await twoFactorRepository.UpdateAsync(twoFactorAuth);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new BackupCodesResponseDto(
            BackupCodes: backupCodes,
            Message: "Backup codes regenerated successfully. Store them securely.");

        logger.LogInformation("Backup codes regenerated successfully. UserId: {UserId}", request.UserId);
        return response;
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
        var codeSpan = code.AsSpan();
        return $"{codeSpan[..4]}-{codeSpan[4..8]}";
    }
}

