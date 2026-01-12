using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.SetUserLanguagePreference;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class SetUserLanguagePreferenceCommandHandler : IRequestHandler<SetUserLanguagePreferenceCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetUserLanguagePreferenceCommandHandler> _logger;

    public SetUserLanguagePreferenceCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<SetUserLanguagePreferenceCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(SetUserLanguagePreferenceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Setting user language preference. UserId: {UserId}, LanguageCode: {LanguageCode}", 
            request.UserId, request.LanguageCode);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var language = await _context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Code.ToLower() == request.LanguageCode.ToLower() && l.IsActive, cancellationToken);

        if (language == null)
        {
            _logger.LogWarning("Language not found. LanguageCode: {LanguageCode}", request.LanguageCode);
            throw new NotFoundException("Dil", Guid.Empty);
        }

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var preference = await _context.Set<UserLanguagePreference>()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (preference == null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            preference = UserLanguagePreference.Create(
                request.UserId,
                language.Id,
                language.Code);
            await _context.Set<UserLanguagePreference>().AddAsync(preference, cancellationToken);
        }
        else
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            preference.UpdateLanguage(language.Id, language.Code);
        }

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User language preference set successfully. UserId: {UserId}, LanguageCode: {LanguageCode}", 
            request.UserId, request.LanguageCode);
        return Unit.Value;
    }
}

