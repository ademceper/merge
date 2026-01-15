using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.LiveCommerce.Commands.EndStream;

public class EndStreamCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<EndStreamCommandHandler> logger) : IRequestHandler<EndStreamCommand, Unit>
{
    public async Task<Unit> Handle(EndStreamCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Ending stream. StreamId: {StreamId}", request.StreamId);

        var stream = await context.Set<LiveStream>()
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream == null)
        {
            logger.LogWarning("Stream not found. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        stream.End();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Stream ended successfully. StreamId: {StreamId}", request.StreamId);
        return Unit.Value;
    }
}
