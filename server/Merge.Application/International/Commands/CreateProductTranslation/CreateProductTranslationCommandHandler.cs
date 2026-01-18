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

namespace Merge.Application.International.Commands.CreateProductTranslation;

public class CreateProductTranslationCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateProductTranslationCommandHandler> logger) : IRequestHandler<CreateProductTranslationCommand, ProductTranslationDto>
{
    public async Task<ProductTranslationDto> Handle(CreateProductTranslationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating product translation. ProductId: {ProductId}, LanguageCode: {LanguageCode}", 
            request.ProductId, request.LanguageCode);

        var language = await context.Set<Language>()
            .FirstOrDefaultAsync(l => EF.Functions.ILike(l.Code, request.LanguageCode), cancellationToken);

        if (language == null)
        {
            logger.LogWarning("Language not found. LanguageCode: {LanguageCode}", request.LanguageCode);
            throw new NotFoundException("Dil", Guid.Empty);
        }

        var exists = await context.Set<ProductTranslation>()
            .AnyAsync(pt => pt.ProductId == request.ProductId &&
                           EF.Functions.ILike(pt.LanguageCode, request.LanguageCode), cancellationToken);

        if (exists)
        {
            logger.LogWarning("Product translation already exists. ProductId: {ProductId}, LanguageCode: {LanguageCode}", 
                request.ProductId, request.LanguageCode);
            throw new BusinessException("Bu ürün ve dil için çeviri zaten mevcut.");
        }

        var translation = ProductTranslation.Create(
            request.ProductId,
            language.Id,
            language.Code,
            request.Name,
            request.Description,
            request.ShortDescription,
            request.MetaTitle,
            request.MetaDescription,
            request.MetaKeywords);

        await context.Set<ProductTranslation>().AddAsync(translation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Product translation created successfully. TranslationId: {TranslationId}", translation.Id);

        return mapper.Map<ProductTranslationDto>(translation);
    }
}
