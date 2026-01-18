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
using Merge.Application.International.Commands.UpdateProductTranslation;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.PatchProductTranslation;

public class PatchProductTranslationCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IMediator mediator,
    ILogger<PatchProductTranslationCommandHandler> logger) : IRequestHandler<PatchProductTranslationCommand, ProductTranslationDto>
{
    public async Task<ProductTranslationDto> Handle(PatchProductTranslationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching product translation. TranslationId: {TranslationId}", request.Id);

        var translation = await context.Set<ProductTranslation>()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (translation is null)
        {
            logger.LogWarning("Product translation not found. TranslationId: {TranslationId}", request.Id);
            throw new NotFoundException("Ürün Çevirisi", request.Id);
        }

        // Get existing values for required fields
        var name = request.PatchDto.Name ?? translation.Name;
        var description = request.PatchDto.Description ?? translation.Description;
        var shortDescription = request.PatchDto.ShortDescription ?? translation.ShortDescription;
        var metaTitle = request.PatchDto.MetaTitle ?? translation.MetaTitle;
        var metaDescription = request.PatchDto.MetaDescription ?? translation.MetaDescription;
        var metaKeywords = request.PatchDto.MetaKeywords ?? translation.MetaKeywords;

        var updateCommand = new UpdateProductTranslationCommand(
            request.Id,
            name,
            description,
            shortDescription,
            metaTitle,
            metaDescription,
            metaKeywords);

        return await mediator.Send(updateCommand, cancellationToken);
    }
}
