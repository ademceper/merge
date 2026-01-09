using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Merge.Application.DTOs.Identity;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Identity.Commands.Verify2FACode;
using Merge.Domain.Entities;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Enums;

namespace Merge.Application.Identity.Commands.Disable2FA;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class Disable2FACommandHandler : IRequestHandler<Disable2FACommand, Unit>
{
    private readonly IRepository<TwoFactorAuth> _twoFactorRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<Disable2FACommandHandler> _logger;

    public Disable2FACommandHandler(
        IRepository<TwoFactorAuth> twoFactorRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<Disable2FACommandHandler> logger)
    {
        _twoFactorRepository = twoFactorRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Unit> Handle(Disable2FACommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Disabling 2FA. UserId: {UserId}", request.UserId);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var twoFactorAuth = await _context.Set<TwoFactorAuth>()
            .FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken);

        if (twoFactorAuth == null || !twoFactorAuth.IsEnabled)
        {
            _logger.LogWarning("2FA disable failed - not enabled. UserId: {UserId}", request.UserId);
            throw new BusinessException("2FA etkin değil.");
        }

        // Verify current 2FA code
        var verifyCommand = new Verify2FACodeCommand(request.UserId, request.DisableDto.Code);
        var isValid = await _mediator.Send(verifyCommand, cancellationToken);

        if (!isValid)
        {
            _logger.LogWarning("2FA disable failed - invalid code. UserId: {UserId}", request.UserId);
            throw new ValidationException("Geçersiz doğrulama kodu.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        twoFactorAuth.Disable();
        await _twoFactorRepository.UpdateAsync(twoFactorAuth);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("2FA disabled successfully. UserId: {UserId}", request.UserId);
        return Unit.Value;
    }
}

