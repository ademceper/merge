using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.LiveCommerce.Commands.LeaveStream;

public class LeaveStreamCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<LeaveStreamCommandHandler> logger) : IRequestHandler<LeaveStreamCommand, Unit>
{
    public async Task<Unit> Handle(LeaveStreamCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Leaving stream. StreamId: {StreamId}, UserId: {UserId}, GuestId: {GuestId}", 
            request.StreamId, request.UserId, request.GuestId);

        var viewer = await context.Set<LiveStreamViewer>()
            .FirstOrDefaultAsync(v => v.LiveStreamId == request.StreamId &&
                (request.UserId.HasValue ? v.UserId == request.UserId : v.GuestId == request.GuestId) &&
                v.IsActive, cancellationToken);

        if (viewer is null)
        {
            logger.LogWarning("Viewer not found in stream. StreamId: {StreamId}, UserId: {UserId}, GuestId: {GuestId}", 
                request.StreamId, request.UserId, request.GuestId);
            throw new NotFoundException("Yayın izleyicisi", Guid.Empty);
        }

        var stream = await context.Set<LiveStream>()
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream is null)
        {
            logger.LogWarning("Stream not found. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        stream.RemoveViewer(viewer.Id);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Left stream successfully. StreamId: {StreamId}, UserId: {UserId}, GuestId: {GuestId}", 
            request.StreamId, request.UserId, request.GuestId);
        return Unit.Value;
    }
}
