using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.LiveCommerce.Commands.PauseStream;

public class PauseStreamCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<PauseStreamCommandHandler> logger) : IRequestHandler<PauseStreamCommand, Unit>
{
    public async Task<Unit> Handle(PauseStreamCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Pausing stream. StreamId: {StreamId}", request.StreamId);

        var stream = await context.Set<LiveStream>()
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream is null)
        {
            logger.LogWarning("Stream not found. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        stream.Pause();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Stream paused successfully. StreamId: {StreamId}", request.StreamId);
        return Unit.Value;
    }
}
