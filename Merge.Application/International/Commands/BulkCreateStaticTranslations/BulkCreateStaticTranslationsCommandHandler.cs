using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.BulkCreateStaticTranslations;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class BulkCreateStaticTranslationsCommandHandler : IRequestHandler<BulkCreateStaticTranslationsCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BulkCreateStaticTranslationsCommandHandler> _logger;

    public BulkCreateStaticTranslationsCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<BulkCreateStaticTranslationsCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(BulkCreateStaticTranslationsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bulk creating static translations. LanguageCode: {LanguageCode}, Count: {Count}", 
            request.LanguageCode, request.Translations.Count);

        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Code.ToLower() == request.LanguageCode.ToLower(), cancellationToken);

        if (language == null)
        {
            _logger.LogWarning("Language not found. LanguageCode: {LanguageCode}", request.LanguageCode);
            throw new NotFoundException("Dil", Guid.Empty);
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !st.IsDeleted (Global Query Filter)
        var existingKeys = await _context.Set<StaticTranslation>()
            .AsNoTracking()
            .Where(st => st.LanguageCode.ToLower() == request.LanguageCode.ToLower())
            .Select(st => st.Key)
            .ToListAsync(cancellationToken);

        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var newTranslations = new List<StaticTranslation>(request.Translations.Count);
        foreach (var kvp in request.Translations)
        {
            if (!existingKeys.Contains(kvp.Key))
            {
                newTranslations.Add(StaticTranslation.Create(
                    kvp.Key,
                    language.Id,
                    language.Code,
                    kvp.Value,
                    "UI"));
            }
        }

        await _context.Set<StaticTranslation>().AddRangeAsync(newTranslations, cancellationToken);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bulk static translations created successfully. Count: {Count}", newTranslations.Count);
        return Unit.Value;
    }
}

