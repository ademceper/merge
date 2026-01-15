using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.LiveCommerce.Commands.CancelStream;

public class CancelStreamCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<CancelStreamCommandHandler> logger) : IRequestHandler<CancelStreamCommand, Unit>
{
    public async Task<Unit> Handle(CancelStreamCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Cancelling stream. StreamId: {StreamId}", request.StreamId);

        var stream = await context.Set<LiveStream>()
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream == null)
        {
            logger.LogWarning("Stream not found. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        stream.Cancel();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Stream cancelled successfully. StreamId: {StreamId}", request.StreamId);
        return Unit.Value;
    }
}
