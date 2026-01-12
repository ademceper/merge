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

namespace Merge.Application.Identity.Commands.SendVerificationCode;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 12.1: Magic Number Sorunu - Configuration kullanımı
public class SendVerificationCodeCommandHandler : IRequestHandler<SendVerificationCodeCommand, Unit>
{
    private readonly Merge.Application.Interfaces.IRepository<TwoFactorCode> _codeRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TwoFactorAuthSettings _twoFactorSettings;
    private readonly IEmailService? _emailService;
    private readonly ISmsService? _smsService;
    private readonly ILogger<SendVerificationCodeCommandHandler> _logger;

    public SendVerificationCodeCommandHandler(
        Merge.Application.Interfaces.IRepository<TwoFactorCode> codeRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        IOptions<TwoFactorAuthSettings> twoFactorSettings,
        ILogger<SendVerificationCodeCommandHandler> logger,
        IEmailService? emailService = null,
        ISmsService? smsService = null)
    {
        _codeRepository = codeRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _twoFactorSettings = twoFactorSettings.Value;
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger;
    }

    public async Task<Unit> Handle(SendVerificationCodeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending verification code. UserId: {UserId}, Purpose: {Purpose}", request.UserId, request.Purpose);

        var twoFactorAuth = await _context.Set<TwoFactorAuth>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken);

        if (twoFactorAuth == null)
        {
            _logger.LogWarning("Send verification code failed - 2FA setup not found. UserId: {UserId}", request.UserId);
            throw new BusinessException("2FA kurulumu yapılmamış.");
        }

        // ✅ BOLUM 12.1: Magic Number Sorunu - Configuration kullanımı
        var code = GenerateNumericCode(_twoFactorSettings.VerificationCodeLength);
        var expiresAt = DateTime.UtcNow.AddMinutes(_twoFactorSettings.VerificationCodeExpirationMinutes);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var twoFactorCode = TwoFactorCode.Create(
            request.UserId,
            code,
            twoFactorAuth.Method,
            expiresAt,
            request.Purpose);

        await _codeRepository.AddAsync(twoFactorCode);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send code via appropriate method
        if (twoFactorAuth.Method == TwoFactorMethod.SMS && !string.IsNullOrEmpty(twoFactorAuth.PhoneNumber))
        {
            if (_smsService == null)
            {
                _logger.LogError("SMS service not configured for sending 2FA code. UserId: {UserId}", request.UserId);
                throw new InvalidOperationException("SMS servisi yapılandırılmamış.");
            }
            // ✅ BOLUM 12.1: Magic Number Sorunu - Configuration kullanımı
            await _smsService.SendSmsAsync(twoFactorAuth.PhoneNumber, $"Your verification code is: {code}. Valid for {_twoFactorSettings.VerificationCodeExpirationMinutes} minutes.");
        }
        else if (twoFactorAuth.Method == TwoFactorMethod.Email && !string.IsNullOrEmpty(twoFactorAuth.Email))
        {
            if (_emailService == null)
            {
                _logger.LogError("Email service not configured for sending 2FA code. UserId: {UserId}", request.UserId);
                throw new InvalidOperationException("Email servisi yapılandırılmamış.");
            }
            // ✅ BOLUM 12.1: Magic Number Sorunu - Configuration kullanımı
            await _emailService.SendEmailAsync(twoFactorAuth.Email, "2FA Verification Code", $"Your verification code is: {code}. This code will expire in {_twoFactorSettings.VerificationCodeExpirationMinutes} minutes.");
        }
        else
        {
            _logger.LogWarning("No valid method to send 2FA code. UserId: {UserId}, Method: {Method}", request.UserId, twoFactorAuth.Method);
            throw new BusinessException("Doğrulama kodu gönderilemedi. Geçerli bir yöntem yapılandırılmamış.");
        }

        _logger.LogInformation("Verification code sent successfully. UserId: {UserId}, Purpose: {Purpose}", request.UserId, request.Purpose);
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

