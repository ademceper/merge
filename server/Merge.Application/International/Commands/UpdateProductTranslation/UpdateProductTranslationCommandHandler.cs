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
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.UpdateProductTranslation;

public class UpdateProductTranslationCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateProductTranslationCommandHandler> logger) : IRequestHandler<UpdateProductTranslationCommand, ProductTranslationDto>
{
    public async Task<ProductTranslationDto> Handle(UpdateProductTranslationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating product translation. TranslationId: {TranslationId}", request.Id);

        var translation = await context.Set<ProductTranslation>()
            .FirstOrDefaultAsync(pt => pt.Id == request.Id, cancellationToken);

        if (translation is null)
        {
            logger.LogWarning("Product translation not found. TranslationId: {TranslationId}", request.Id);
            throw new NotFoundException("Ürün çevirisi", request.Id);
        }

        translation.Update(
            request.Name,
            request.Description,
            request.ShortDescription,
            request.MetaTitle,
            request.MetaDescription,
            request.MetaKeywords);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Product translation updated successfully. TranslationId: {TranslationId}", translation.Id);

        return mapper.Map<ProductTranslationDto>(translation);
    }
}
