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

namespace Merge.Application.International.Commands.CreateCategoryTranslation;

public class CreateCategoryTranslationCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateCategoryTranslationCommandHandler> logger) : IRequestHandler<CreateCategoryTranslationCommand, CategoryTranslationDto>
{
    public async Task<CategoryTranslationDto> Handle(CreateCategoryTranslationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating category translation. CategoryId: {CategoryId}, LanguageCode: {LanguageCode}", 
            request.CategoryId, request.LanguageCode);

        var language = await context.Set<Language>()
            .FirstOrDefaultAsync(l => EF.Functions.ILike(l.Code, request.LanguageCode), cancellationToken);

        if (language == null)
        {
            logger.LogWarning("Language not found. LanguageCode: {LanguageCode}", request.LanguageCode);
            throw new NotFoundException("Dil", Guid.Empty);
        }

        var exists = await context.Set<CategoryTranslation>()
            .AnyAsync(ct => ct.CategoryId == request.CategoryId &&
                           EF.Functions.ILike(ct.LanguageCode, request.LanguageCode), cancellationToken);

        if (exists)
        {
            logger.LogWarning("Category translation already exists. CategoryId: {CategoryId}, LanguageCode: {LanguageCode}", 
                request.CategoryId, request.LanguageCode);
            throw new BusinessException("Bu kategori ve dil için çeviri zaten mevcut.");
        }

        var translation = CategoryTranslation.Create(
            request.CategoryId,
            language.Id,
            language.Code,
            request.Name,
            request.Description);

        await context.Set<CategoryTranslation>().AddAsync(translation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Category translation created successfully. TranslationId: {TranslationId}", translation.Id);

        return mapper.Map<CategoryTranslationDto>(translation);
    }
}
