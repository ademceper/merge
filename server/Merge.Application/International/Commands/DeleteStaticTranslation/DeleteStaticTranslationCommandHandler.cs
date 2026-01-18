using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.SharedKernel;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.DeleteStaticTranslation;

public class DeleteStaticTranslationCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteStaticTranslationCommandHandler> logger) : IRequestHandler<DeleteStaticTranslationCommand, Unit>
{
    public async Task<Unit> Handle(DeleteStaticTranslationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting static translation. TranslationId: {TranslationId}", request.Id);

        var translation = await context.Set<StaticTranslation>()
            .FirstOrDefaultAsync(st => st.Id == request.Id, cancellationToken);

        if (translation is null)
        {
            logger.LogWarning("Static translation not found for deletion. TranslationId: {TranslationId}", request.Id);
            throw new NotFoundException("Statik çeviri", request.Id);
        }

        translation.MarkAsDeleted(); // StaticTranslation entity'sinde domain method

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Static translation deleted successfully. TranslationId: {TranslationId}", translation.Id);
        return Unit.Value;
    }
}
