using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<TwoFactorAuth>;

namespace Merge.Application.Identity.Commands.VerifyBackupCode;

public class VerifyBackupCodeCommandHandler(
    IRepository twoFactorRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<VerifyBackupCodeCommandHandler> logger) : IRequestHandler<VerifyBackupCodeCommand, bool>
{

    public async Task<bool> Handle(VerifyBackupCodeCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Verifying backup code. UserId: {UserId}", request.UserId);

        var twoFactorAuth = await context.Set<TwoFactorAuth>()
            .FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken);

        if (twoFactorAuth == null || !twoFactorAuth.IsEnabled || twoFactorAuth.BackupCodes == null)
        {
            logger.LogWarning("Backup code verification failed - 2FA not enabled or no backup codes. UserId: {UserId}", request.UserId);
            return false;
        }

        var normalizedCode = request.BackupCode.Replace("-", "", StringComparison.OrdinalIgnoreCase).ToUpperInvariant();

        var normalizedBackupCodes = twoFactorAuth.BackupCodes
            .Select(c => c.Replace("-", "", StringComparison.OrdinalIgnoreCase).ToUpperInvariant())
            .ToArray();
        
        var matchingIndex = Array.IndexOf(normalizedBackupCodes, normalizedCode);
        string? matchingCode = matchingIndex >= 0 ? twoFactorAuth.BackupCodes[matchingIndex] : null;
        
        if (matchingCode != null)
        {
            twoFactorAuth.RemoveBackupCode(matchingCode);

            await twoFactorRepository.UpdateAsync(twoFactorAuth);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Backup code verified successfully. UserId: {UserId}", request.UserId);
            return true;
        }

        logger.LogWarning("Backup code verification failed - invalid code. UserId: {UserId}", request.UserId);
        return false;
    }
}

