using Microsoft.AspNetCore.Identity;
using Merge.Application.Services.Notification;
using UserEntity = Merge.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.Identity;
using Merge.Application.Interfaces.User;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;

namespace Merge.Application.Services.Identity;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly IRepository<EmailVerification> _emailVerificationRepository;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IEmailService? _emailService;
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public EmailVerificationService(
        IRepository<EmailVerification> emailVerificationRepository,
        UserManager<UserEntity> userManager,
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IEmailService? emailService = null)
    {
        _emailVerificationRepository = emailVerificationRepository;
        _userManager = userManager;
        _context = context;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<string> GenerateVerificationTokenAsync(Guid userId, string email)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", userId);
        }

        // ✅ PERFORMANCE: Bulk delete ile N+1 query önlenir
        // ✅ PERFORMANCE: !ev.IsVerified kontrolü IsDeleted değil, farklı property (kabul edilebilir)
        var oldVerifications = _context.EmailVerifications
            .Where(ev => ev.UserId == userId && !ev.IsVerified);

        _context.EmailVerifications.RemoveRange(oldVerifications);

        // Yeni token oluştur
        var token = Guid.NewGuid().ToString("N");
        var verification = new EmailVerification
        {
            UserId = userId,
            Email = email,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24), // 24 saat geçerli
            IsVerified = false
        };

        await _emailVerificationRepository.AddAsync(verification);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync();

        // Email gönder
        if (_emailService != null)
        {
            var verificationUrl = $"/verify-email?token={token}";
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

    public async Task<bool> VerifyEmailAsync(string token)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var verification = await _context.EmailVerifications
            .Include(ev => ev.User)
            .FirstOrDefaultAsync(ev => ev.Token == token);

        if (verification == null)
        {
            return false;
        }

        if (verification.IsVerified)
        {
            return true; // Zaten doğrulanmış
        }

        if (verification.ExpiresAt < DateTime.UtcNow)
        {
            throw new BusinessException("Doğrulama linki süresi dolmuş.");
        }

        // Email'i doğrula
        verification.IsVerified = true;
        verification.VerifiedAt = DateTime.UtcNow;
        await _emailVerificationRepository.UpdateAsync(verification);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync();

        // Kullanıcının email doğrulama durumunu güncelle
        verification.User.EmailConfirmed = true;
        await _userManager.UpdateAsync(verification.User);

        return true;
    }

    public async Task<bool> ResendVerificationEmailAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", userId);
        }

        if (user.EmailConfirmed)
        {
            throw new BusinessException("Email zaten doğrulanmış.");
        }

        var token = await GenerateVerificationTokenAsync(userId, user.Email ?? string.Empty);
        return !string.IsNullOrEmpty(token);
    }

    public async Task<bool> IsEmailVerifiedAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.EmailConfirmed ?? false;
    }
}

