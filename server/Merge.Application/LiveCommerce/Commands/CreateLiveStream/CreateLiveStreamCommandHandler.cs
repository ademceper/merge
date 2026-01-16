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

namespace Merge.Application.LiveCommerce.Commands.CreateLiveStream;

public class CreateLiveStreamCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateLiveStreamCommandHandler> logger) : IRequestHandler<CreateLiveStreamCommand, LiveStreamDto>
{
    public async Task<LiveStreamDto> Handle(CreateLiveStreamCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating live stream. SellerId: {SellerId}, Title: {Title}", request.SellerId, request.Title);

        var stream = LiveStream.Create(
            request.SellerId,
            request.Title,
            request.Description,
            request.ScheduledStartTime,
            request.StreamUrl,
            request.StreamKey,
            request.ThumbnailUrl,
            request.Category,
            request.Tags);

        await context.Set<LiveStream>().AddAsync(stream, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var createdStream = await context.Set<LiveStream>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .Include(s => s.Products)
                .ThenInclude(p => p.Product)
            .FirstOrDefaultAsync(s => s.Id == stream.Id, cancellationToken);

        if (createdStream == null)
        {
            logger.LogWarning("Live stream not found after creation. StreamId: {StreamId}", stream.Id);
            throw new NotFoundException("Canlı yayın", stream.Id);
        }

        logger.LogInformation("Live stream created successfully. StreamId: {StreamId}, SellerId: {SellerId}", stream.Id, request.SellerId);

        return mapper.Map<LiveStreamDto>(createdStream);
    }
}
