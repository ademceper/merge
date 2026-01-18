using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.CreateStaticTranslation;

public class CreateStaticTranslationCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateStaticTranslationCommandHandler> logger) : IRequestHandler<CreateStaticTranslationCommand, StaticTranslationDto>
{
    public async Task<StaticTranslationDto> Handle(CreateStaticTranslationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating static translation. Key: {Key}, LanguageCode: {LanguageCode}", 
            request.Key, request.LanguageCode);

        var language = await context.Set<Language>()
            .FirstOrDefaultAsync(l => EF.Functions.ILike(l.Code, request.LanguageCode), cancellationToken);

        if (language == null)
        {
            logger.LogWarning("Language not found. LanguageCode: {LanguageCode}", request.LanguageCode);
            throw new NotFoundException("Dil", Guid.Empty);
        }

        var exists = await context.Set<StaticTranslation>()
            .AnyAsync(st => st.Key == request.Key &&
                           EF.Functions.ILike(st.LanguageCode, request.LanguageCode), cancellationToken);

        if (exists)
        {
            logger.LogWarning("Static translation already exists. Key: {Key}, LanguageCode: {LanguageCode}", 
                request.Key, request.LanguageCode);
            throw new BusinessException("Bu anahtar ve dil için çeviri zaten mevcut.");
        }

        var translation = StaticTranslation.Create(
            request.Key,
            language.Id,
            language.Code,
            request.Value,
            request.Category);

        await context.Set<StaticTranslation>().AddAsync(translation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Static translation created successfully. TranslationId: {TranslationId}", translation.Id);

        return mapper.Map<StaticTranslationDto>(translation);
    }
}
