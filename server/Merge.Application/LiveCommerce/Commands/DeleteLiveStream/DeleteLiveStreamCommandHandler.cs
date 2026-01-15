using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.LiveCommerce.Commands.DeleteLiveStream;

public class DeleteLiveStreamCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteLiveStreamCommandHandler> logger) : IRequestHandler<DeleteLiveStreamCommand, Unit>
{
    public async Task<Unit> Handle(DeleteLiveStreamCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting live stream. StreamId: {StreamId}", request.StreamId);

        var stream = await context.Set<LiveStream>()
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream == null)
        {
            logger.LogWarning("Stream not found for deletion. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        stream.MarkAsDeleted();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Live stream deleted successfully. StreamId: {StreamId}", request.StreamId);
        return Unit.Value;
    }
}
