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

namespace Merge.Application.International.Commands.PatchStaticTranslation;

public class PatchStaticTranslationCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IMediator mediator,
    ILogger<PatchStaticTranslationCommandHandler> logger) : IRequestHandler<PatchStaticTranslationCommand, StaticTranslationDto>
{
    public async Task<StaticTranslationDto> Handle(PatchStaticTranslationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching static translation. TranslationId: {TranslationId}", request.Id);

        var translation = await context.Set<StaticTranslation>()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (translation == null)
        {
            logger.LogWarning("Static translation not found. TranslationId: {TranslationId}", request.Id);
            throw new NotFoundException("Statik Çeviri", request.Id);
        }

        var value = request.PatchDto.Value ?? translation.Value;
        var category = request.PatchDto.Category ?? translation.Category;

        var updateCommand = new UpdateStaticTranslationCommand(request.Id, value, category);

        return await mediator.Send(updateCommand, cancellationToken);
    }
}
