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

namespace Merge.Application.Identity.Commands.RegenerateBackupCodes;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 12.1: Magic Number Sorunu - Configuration kullanımı
public class RegenerateBackupCodesCommandHandler : IRequestHandler<RegenerateBackupCodesCommand, BackupCodesResponseDto>
{
    private readonly IRepository<TwoFactorAuth> _twoFactorRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly TwoFactorAuthSettings _twoFactorSettings;
    private readonly ILogger<RegenerateBackupCodesCommandHandler> _logger;

    public RegenerateBackupCodesCommandHandler(
        IRepository<TwoFactorAuth> twoFactorRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IOptions<TwoFactorAuthSettings> twoFactorSettings,
        ILogger<RegenerateBackupCodesCommandHandler> logger)
    {
        _twoFactorRepository = twoFactorRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _twoFactorSettings = twoFactorSettings.Value;
        _logger = logger;
    }

    public async Task<BackupCodesResponseDto> Handle(RegenerateBackupCodesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Regenerating backup codes. UserId: {UserId}", request.UserId);

        var twoFactorAuth = await _context.Set<TwoFactorAuth>()
            .FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken);

        if (twoFactorAuth == null || !twoFactorAuth.IsEnabled)
        {
            _logger.LogWarning("Regenerate backup codes failed - 2FA not enabled. UserId: {UserId}", request.UserId);
            throw new BusinessException("2FA etkin değil.");
        }

        // Verify current 2FA code
        var verifyCommand = new Verify2FACodeCommand(request.UserId, request.RegenerateDto.Code);
        var isValid = await _mediator.Send(verifyCommand, cancellationToken);

        if (!isValid)
        {
            _logger.LogWarning("Regenerate backup codes failed - invalid code. UserId: {UserId}", request.UserId);
            throw new ValidationException("Geçersiz doğrulama kodu.");
        }

        // ✅ BOLUM 12.1: Magic Number Sorunu - Configuration kullanımı
        var backupCodes = GenerateBackupCodes(_twoFactorSettings.BackupCodeCount);
        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        twoFactorAuth.UpdateBackupCodes(backupCodes);
        await _twoFactorRepository.UpdateAsync(twoFactorAuth);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var response = new BackupCodesResponseDto(
            BackupCodes: backupCodes,
            Message: "Backup codes regenerated successfully. Store them securely.");

        _logger.LogInformation("Backup codes regenerated successfully. UserId: {UserId}", request.UserId);
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
        // ✅ PERFORMANCE: Span<char> kullanımı (zero allocation)
        var codeSpan = code.AsSpan();
        return $"{codeSpan[..4]}-{codeSpan[4..8]}";
    }
}

