using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.LiveCommerce.Commands.StartStream;

public class StartStreamCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<StartStreamCommandHandler> logger) : IRequestHandler<StartStreamCommand, Unit>
{
    public async Task<Unit> Handle(StartStreamCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting stream. StreamId: {StreamId}", request.StreamId);

        var stream = await context.Set<LiveStream>()
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream == null)
        {
            logger.LogWarning("Stream not found. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        stream.Start();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Stream started successfully. StreamId: {StreamId}", request.StreamId);
        return Unit.Value;
    }
}
