using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.DeleteCategoryTranslation;

public class DeleteCategoryTranslationCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteCategoryTranslationCommandHandler> logger) : IRequestHandler<DeleteCategoryTranslationCommand, Unit>
{
    public async Task<Unit> Handle(DeleteCategoryTranslationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting category translation. TranslationId: {TranslationId}", request.Id);

        var translation = await context.Set<CategoryTranslation>()
            .FirstOrDefaultAsync(ct => ct.Id == request.Id, cancellationToken);

        if (translation == null)
        {
            logger.LogWarning("Category translation not found for deletion. TranslationId: {TranslationId}", request.Id);
            throw new NotFoundException("Kategori çevirisi", request.Id);
        }

        translation.MarkAsDeleted(); // CategoryTranslation entity'sinde domain method

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Category translation deleted successfully. TranslationId: {TranslationId}", translation.Id);
        return Unit.Value;
    }
}
