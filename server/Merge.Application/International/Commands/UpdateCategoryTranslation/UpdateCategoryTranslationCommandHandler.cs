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

namespace Merge.Application.International.Commands.UpdateCategoryTranslation;

public class UpdateCategoryTranslationCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateCategoryTranslationCommandHandler> logger) : IRequestHandler<UpdateCategoryTranslationCommand, CategoryTranslationDto>
{
    public async Task<CategoryTranslationDto> Handle(UpdateCategoryTranslationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating category translation. TranslationId: {TranslationId}", request.Id);

        var translation = await context.Set<CategoryTranslation>()
            .FirstOrDefaultAsync(ct => ct.Id == request.Id, cancellationToken);

        if (translation == null)
        {
            logger.LogWarning("Category translation not found. TranslationId: {TranslationId}", request.Id);
            throw new NotFoundException("Kategori çevirisi", request.Id);
        }

        translation.Update(request.Name, request.Description);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Category translation updated successfully. TranslationId: {TranslationId}", translation.Id);

        return mapper.Map<CategoryTranslationDto>(translation);
    }
}
