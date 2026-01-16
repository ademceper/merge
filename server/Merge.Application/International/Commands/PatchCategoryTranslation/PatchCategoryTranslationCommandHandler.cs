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
using Merge.Application.International.Commands.UpdateCategoryTranslation;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.PatchCategoryTranslation;

public class PatchCategoryTranslationCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IMediator mediator,
    ILogger<PatchCategoryTranslationCommandHandler> logger) : IRequestHandler<PatchCategoryTranslationCommand, CategoryTranslationDto>
{
    public async Task<CategoryTranslationDto> Handle(PatchCategoryTranslationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching category translation. TranslationId: {TranslationId}", request.Id);

        var translation = await context.Set<CategoryTranslation>()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (translation == null)
        {
            logger.LogWarning("Category translation not found. TranslationId: {TranslationId}", request.Id);
            throw new NotFoundException("Kategori Çevirisi", request.Id);
        }

        var name = request.PatchDto.Name ?? translation.Name;
        var description = request.PatchDto.Description ?? translation.Description;

        var updateCommand = new UpdateCategoryTranslationCommand(request.Id, name, description);

        return await mediator.Send(updateCommand, cancellationToken);
    }
}
