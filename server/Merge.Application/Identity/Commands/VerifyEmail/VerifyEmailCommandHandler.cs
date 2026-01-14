using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.SharedKernel.DomainEvents;
using UserEntity = Merge.Domain.Modules.Identity.User;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Identity.Commands.VerifyEmail;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Unit>
{
    private readonly Merge.Application.Interfaces.IRepository<EmailVerification> _emailVerificationRepository;
    private readonly IDbContext _context;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VerifyEmailCommandHandler> _logger;

    public VerifyEmailCommandHandler(
        Merge.Application.Interfaces.IRepository<EmailVerification> emailVerificationRepository,
        IDbContext context,
        UserManager<UserEntity> userManager,
        IUnitOfWork unitOfWork,
        ILogger<VerifyEmailCommandHandler> logger)
    {
        _emailVerificationRepository = emailVerificationRepository;
        _context = context;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Email verification attempt. Token: {Token}", request.Token);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var verification = await _context.Set<EmailVerification>()
            .Include(ev => ev.User)
            .FirstOrDefaultAsync(ev => ev.Token == request.Token, cancellationToken);

        if (verification == null)
        {
            _logger.LogWarning("Email verification failed - invalid token. Token: {Token}", request.Token);
            throw new BusinessException("Geçersiz token.");
        }

        if (verification.IsVerified)
        {
            _logger.LogInformation("Email already verified. UserId: {UserId}", verification.UserId);
            return Unit.Value; // Zaten doğrulanmış
        }

        if (verification.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Email verification failed - token expired. Token: {Token}, ExpiresAt: {ExpiresAt}", request.Token, verification.ExpiresAt);
            throw new BusinessException("Doğrulama linki süresi dolmuş.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        // Email'i doğrula (Domain Method içinde domain event de ekleniyor)
        verification.Verify();
        await _emailVerificationRepository.UpdateAsync(verification);
        
        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        // Kullanıcının email doğrulama durumunu güncelle (Domain Method kullan)
        verification.User.ConfirmEmail();
        
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // UserManager'ın kendi context'ini kullanması için UpdateAsync çağrısı gerekli
        await _userManager.UpdateAsync(verification.User);

        _logger.LogInformation("Email verified successfully. UserId: {UserId}, Email: {Email}", verification.UserId, verification.Email);
        return Unit.Value;
    }
}

