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

namespace Merge.Application.International.Commands.UpdateStaticTranslation;

public class UpdateStaticTranslationCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateStaticTranslationCommandHandler> logger) : IRequestHandler<UpdateStaticTranslationCommand, StaticTranslationDto>
{
    public async Task<StaticTranslationDto> Handle(UpdateStaticTranslationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating static translation. TranslationId: {TranslationId}", request.Id);

        var translation = await context.Set<StaticTranslation>()
            .FirstOrDefaultAsync(st => st.Id == request.Id, cancellationToken);

        if (translation == null)
        {
            logger.LogWarning("Static translation not found. TranslationId: {TranslationId}", request.Id);
            throw new NotFoundException("Statik çeviri", request.Id);
        }

        translation.Update(request.Value, request.Category);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Static translation updated successfully. TranslationId: {TranslationId}", translation.Id);

        return mapper.Map<StaticTranslationDto>(translation);
    }
}
