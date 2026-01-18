using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.LiveCommerce;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.LiveCommerce.Commands.UpdateLiveStream;

public class UpdateLiveStreamCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateLiveStreamCommandHandler> logger) : IRequestHandler<UpdateLiveStreamCommand, LiveStreamDto>
{
    public async Task<LiveStreamDto> Handle(UpdateLiveStreamCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating live stream. StreamId: {StreamId}", request.StreamId);

        var stream = await context.Set<LiveStream>()
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream is null)
        {
            logger.LogWarning("Stream not found. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        stream.UpdateDetails(
            request.Title,
            request.Description,
            request.ScheduledStartTime,
            request.StreamUrl,
            request.StreamKey,
            request.ThumbnailUrl,
            request.Category,
            request.Tags);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedStream = await context.Set<LiveStream>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .Include(s => s.Products)
                .ThenInclude(p => p.Product)
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (updatedStream is null)
        {
            logger.LogWarning("Stream not found after update. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        logger.LogInformation("Live stream updated successfully. StreamId: {StreamId}", request.StreamId);

        return mapper.Map<LiveStreamDto>(updatedStream);
    }
}
