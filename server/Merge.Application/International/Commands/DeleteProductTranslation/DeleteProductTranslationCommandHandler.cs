using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.DeleteProductTranslation;

public class DeleteProductTranslationCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteProductTranslationCommandHandler> logger) : IRequestHandler<DeleteProductTranslationCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProductTranslationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting product translation. TranslationId: {TranslationId}", request.Id);

        var translation = await context.Set<ProductTranslation>()
            .FirstOrDefaultAsync(pt => pt.Id == request.Id, cancellationToken);

        if (translation == null)
        {
            logger.LogWarning("Product translation not found for deletion. TranslationId: {TranslationId}", request.Id);
            throw new NotFoundException("Ürün çevirisi", request.Id);
        }

        translation.MarkAsDeleted();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Product translation deleted successfully. TranslationId: {TranslationId}", translation.Id);
        return Unit.Value;
    }
}
