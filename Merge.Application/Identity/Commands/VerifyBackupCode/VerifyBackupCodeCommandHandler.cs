using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Identity.Commands.VerifyBackupCode;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class VerifyBackupCodeCommandHandler : IRequestHandler<VerifyBackupCodeCommand, bool>
{
    private readonly IRepository<TwoFactorAuth> _twoFactorRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VerifyBackupCodeCommandHandler> _logger;

    public VerifyBackupCodeCommandHandler(
        IRepository<TwoFactorAuth> twoFactorRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<VerifyBackupCodeCommandHandler> logger)
    {
        _twoFactorRepository = twoFactorRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(VerifyBackupCodeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Verifying backup code. UserId: {UserId}", request.UserId);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var twoFactorAuth = await _context.Set<TwoFactorAuth>()
            .FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken);

        if (twoFactorAuth == null || !twoFactorAuth.IsEnabled || twoFactorAuth.BackupCodes == null)
        {
            _logger.LogWarning("Backup code verification failed - 2FA not enabled or no backup codes. UserId: {UserId}", request.UserId);
            return false;
        }

        // ✅ PERFORMANCE: Normalize backup code once (avoid repeated string operations)
        var normalizedCode = request.BackupCode.Replace("-", "", StringComparison.OrdinalIgnoreCase).ToUpperInvariant();

        // ✅ PERFORMANCE: Pre-normalize all backup codes once for comparison (avoid repeated string operations)
        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU) - Backup codes array'i zaten sabit boyutlu (10 eleman)
        var normalizedBackupCodes = twoFactorAuth.BackupCodes
            .Select(c => c.Replace("-", "", StringComparison.OrdinalIgnoreCase).ToUpperInvariant())
            .ToArray();
        
        var matchingIndex = Array.IndexOf(normalizedBackupCodes, normalizedCode);
        string? matchingCode = matchingIndex >= 0 ? twoFactorAuth.BackupCodes[matchingIndex] : null;
        
        if (matchingCode != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            twoFactorAuth.RemoveBackupCode(matchingCode);

            await _twoFactorRepository.UpdateAsync(twoFactorAuth);
            // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
            // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Backup code verified successfully. UserId: {UserId}", request.UserId);
            return true;
        }

        _logger.LogWarning("Backup code verification failed - invalid code. UserId: {UserId}", request.UserId);
        return false;
    }
}

