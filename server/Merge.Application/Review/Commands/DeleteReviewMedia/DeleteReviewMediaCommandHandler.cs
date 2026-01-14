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
public class DeleteReviewMediaCommandHandler : IRequestHandler<DeleteReviewMediaCommand>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteReviewMediaCommandHandler> _logger;

    public DeleteReviewMediaCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteReviewMediaCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(DeleteReviewMediaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting review media. MediaId: {MediaId}", request.MediaId);

        // ✅ PERFORMANCE: FindAsync yerine FirstOrDefaultAsync (Global Query Filter)
        var media = await _context.Set<ReviewMedia>()
            .FirstOrDefaultAsync(m => m.Id == request.MediaId, cancellationToken);

        if (media != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            media.MarkAsDeleted();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Review media deleted successfully. MediaId: {MediaId}", request.MediaId);
        }
    }
}
