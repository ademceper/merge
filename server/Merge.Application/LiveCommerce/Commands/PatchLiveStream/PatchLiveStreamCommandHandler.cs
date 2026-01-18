using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.LiveCommerce;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.LiveCommerce.Commands.PatchLiveStream;

/// <summary>
/// Handler for PatchLiveStreamCommand
/// HIGH-API-001: PATCH Support - Partial updates implementation
/// </summary>
public class PatchLiveStreamCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<PatchLiveStreamCommandHandler> logger) : IRequestHandler<PatchLiveStreamCommand, LiveStreamDto>
{
    public async Task<LiveStreamDto> Handle(PatchLiveStreamCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching live stream. StreamId: {StreamId}", request.StreamId);

        var stream = await context.Set<LiveStream>()
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream is null)
        {
            logger.LogWarning("Live stream not found. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        // Apply partial updates - get existing values if not provided
        var title = request.PatchDto.Title ?? stream.Title;
        var description = request.PatchDto.Description ?? stream.Description;
        var scheduledStartTime = request.PatchDto.ScheduledStartTime ?? stream.ScheduledStartTime;
        var streamUrl = request.PatchDto.StreamUrl ?? stream.StreamUrl;
        var streamKey = request.PatchDto.StreamKey ?? stream.StreamKey;
        var thumbnailUrl = request.PatchDto.ThumbnailUrl ?? stream.ThumbnailUrl;
        var category = request.PatchDto.Category ?? stream.Category;
        var tags = request.PatchDto.Tags ?? stream.Tags;

        stream.UpdateDetails(
            title,
            description ?? string.Empty,
            scheduledStartTime,
            streamUrl,
            streamKey,
            thumbnailUrl,
            category,
            tags);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Live stream patched successfully. StreamId: {StreamId}", request.StreamId);

        return mapper.Map<LiveStreamDto>(stream);
    }
}
