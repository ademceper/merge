using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Commands.DeleteReviewMedia;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteReviewMediaCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<DeleteReviewMediaCommandHandler> logger) : IRequestHandler<DeleteReviewMediaCommand>
{

    public async Task Handle(DeleteReviewMediaCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting review media. MediaId: {MediaId}", request.MediaId);

        // ✅ PERFORMANCE: FindAsync yerine FirstOrDefaultAsync (Global Query Filter)
        var media = await context.Set<ReviewMedia>()
            .FirstOrDefaultAsync(m => m.Id == request.MediaId, cancellationToken);

        if (media != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            media.MarkAsDeleted();
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Review media deleted successfully. MediaId: {MediaId}", request.MediaId);
        }
    }
}
