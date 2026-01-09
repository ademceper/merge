using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using UserEntity = Merge.Domain.Entities.User;

namespace Merge.Application.Identity.Commands.ResendVerificationEmail;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class ResendVerificationEmailCommandHandler : IRequestHandler<ResendVerificationEmailCommand, Unit>
{
    private readonly IRepository<EmailVerification> _emailVerificationRepository;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService? _emailService;
    private readonly ILogger<ResendVerificationEmailCommandHandler> _logger;
    private readonly EmailSettings _emailSettings;

    public ResendVerificationEmailCommandHandler(
        IRepository<EmailVerification> emailVerificationRepository,
        UserManager<UserEntity> userManager,
        IDbContext context,
        IUnitOfWork unitOfWork,
        IOptions<EmailSettings> emailSettings,
        ILogger<ResendVerificationEmailCommandHandler> logger,
        IEmailService? emailService = null)
    {
        _emailVerificationRepository = emailVerificationRepository;
        _userManager = userManager;
        _context = context;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
        _emailSettings = emailSettings.Value;
    }

    public async Task<Unit> Handle(ResendVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Resend verification email attempt. UserId: {UserId}", request.UserId);

        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            _logger.LogWarning("Resend verification email failed - user not found. UserId: {UserId}", request.UserId);
            throw new NotFoundException("Kullanıcı", request.UserId);
        }

        if (user.EmailConfirmed)
        {
            _logger.LogInformation("Resend verification email skipped - email already verified. UserId: {UserId}", request.UserId);
            throw new BusinessException("Email zaten doğrulanmış.");
        }

        // Generate verification token
        await GenerateVerificationTokenAsync(request.UserId, user.Email ?? string.Empty, cancellationToken);
        
        _logger.LogInformation("Verification email resent successfully. UserId: {UserId}", request.UserId);
        return Unit.Value;
    }

    private async Task<string> GenerateVerificationTokenAsync(Guid userId, string email, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", userId);
        }

        // ✅ PERFORMANCE: Bulk delete ile N+1 query önlenir
        // ✅ PERFORMANCE: RemoveRange için önce ToListAsync() çağrılmalı (EF Core requirement)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle (maksimum 100 eski verification)
        var oldVerifications = await _context.Set<EmailVerification>()
            .Where(ev => ev.UserId == userId && !ev.IsVerified)
            .OrderBy(ev => ev.CreatedAt) // En eski olanları sil
            .Take(100) // ✅ Güvenlik: Maksimum 100 eski verification sil
            .ToListAsync(cancellationToken);

        _context.Set<EmailVerification>().RemoveRange(oldVerifications);

        // Yeni token oluştur
        var token = Guid.NewGuid().ToString("N");
        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var verification = EmailVerification.Create(
            userId,
            email,
            token,
            DateTime.UtcNow.AddHours(_emailSettings.VerificationTokenExpirationHours));

        await _emailVerificationRepository.AddAsync(verification);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Email gönder
        if (_emailService != null)
        {
            // ✅ BOLUM 12.1: Magic Number Sorunu - Configuration kullanımı
            var verificationUrl = $"{_emailSettings.VerificationUrlPath}?token={token}";
            var emailBody = $@"
                <h2>Email Doğrulama</h2>
                <p>Hesabınızı doğrulamak için aşağıdaki linke tıklayın:</p>
                <p><a href=""{verificationUrl}"">Email'i Doğrula</a></p>
                <p>Bu link 24 saat geçerlidir.</p>
            ";
            await _emailService.SendEmailAsync(email, "Email Doğrulama", emailBody);
        }

        return token;
    }
}

