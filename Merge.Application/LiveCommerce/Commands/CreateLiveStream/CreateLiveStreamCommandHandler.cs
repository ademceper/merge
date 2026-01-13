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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class CreateLiveStreamCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateLiveStreamCommandHandler> logger) : IRequestHandler<CreateLiveStreamCommand, LiveStreamDto>
{

    public async Task<LiveStreamDto> Handle(CreateLiveStreamCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating live stream. SellerId: {SellerId}, Title: {Title}", request.SellerId, request.Title);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
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

        // ✅ PERFORMANCE: AsNoTracking + Include ile tek query'de getir
        // ✅ PERFORMANCE: AsSplitQuery ile Cartesian Explosion önlenir (birden fazla Include var)
        var createdStream = await context.Set<LiveStream>()
            .AsNoTracking()
            .AsSplitQuery() // ✅ EF Core 9: Query splitting - her Include ayrı sorgu
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

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<LiveStreamDto>(createdStream);
    }
}

