using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.LiveCommerce;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.LiveCommerce.Commands.UpdateLiveStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class UpdateLiveStreamCommandHandler : IRequestHandler<UpdateLiveStreamCommand, LiveStreamDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateLiveStreamCommandHandler> _logger;

    public UpdateLiveStreamCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateLiveStreamCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LiveStreamDto> Handle(UpdateLiveStreamCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating live stream. StreamId: {StreamId}", request.StreamId);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var stream = await _context.Set<LiveStream>()
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream == null)
        {
            _logger.LogWarning("Stream not found. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        stream.UpdateDetails(
            request.Title,
            request.Description,
            request.ScheduledStartTime,
            request.StreamUrl,
            request.StreamKey,
            request.ThumbnailUrl,
            request.Category,
            request.Tags);

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking + Include ile tek query'de getir
        var updatedStream = await _context.Set<LiveStream>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .Include(s => s.Products)
                .ThenInclude(p => p.Product)
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (updatedStream == null)
        {
            _logger.LogWarning("Stream not found after update. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        _logger.LogInformation("Live stream updated successfully. StreamId: {StreamId}", request.StreamId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<LiveStreamDto>(updatedStream);
    }
}

