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

public class BulkCreateStaticTranslationsCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<BulkCreateStaticTranslationsCommandHandler> logger) : IRequestHandler<BulkCreateStaticTranslationsCommand, Unit>
{
    public async Task<Unit> Handle(BulkCreateStaticTranslationsCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Bulk creating static translations. LanguageCode: {LanguageCode}, Count: {Count}", 
            request.LanguageCode, request.Translations.Count);

        var language = await context.Set<Language>()
            .FirstOrDefaultAsync(l => EF.Functions.ILike(l.Code, request.LanguageCode), cancellationToken);

        if (language is null)
        {
            logger.LogWarning("Language not found. LanguageCode: {LanguageCode}", request.LanguageCode);
            throw new NotFoundException("Dil", Guid.Empty);
        }

        var existingKeys = await context.Set<StaticTranslation>()
            .AsNoTracking()
            .Where(st => EF.Functions.ILike(st.LanguageCode, request.LanguageCode))
            .Select(st => st.Key)
            .ToListAsync(cancellationToken);

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

        await context.Set<StaticTranslation>().AddRangeAsync(newTranslations, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Bulk static translations created successfully. Count: {Count}", newTranslations.Count);
        return Unit.Value;
    }
}
