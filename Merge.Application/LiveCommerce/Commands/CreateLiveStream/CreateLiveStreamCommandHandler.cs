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

namespace Merge.Application.LiveCommerce.Commands.CreateLiveStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class CreateLiveStreamCommandHandler : IRequestHandler<CreateLiveStreamCommand, LiveStreamDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateLiveStreamCommandHandler> _logger;

    public CreateLiveStreamCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateLiveStreamCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LiveStreamDto> Handle(CreateLiveStreamCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating live stream. SellerId: {SellerId}, Title: {Title}", request.SellerId, request.Title);

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

        await _context.Set<LiveStream>().AddAsync(stream, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking + Include ile tek query'de getir
        var createdStream = await _context.Set<LiveStream>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .Include(s => s.Products)
                .ThenInclude(p => p.Product)
            .FirstOrDefaultAsync(s => s.Id == stream.Id, cancellationToken);

        if (createdStream == null)
        {
            _logger.LogWarning("Live stream not found after creation. StreamId: {StreamId}", stream.Id);
            throw new NotFoundException("Canlı yayın", stream.Id);
        }

        _logger.LogInformation("Live stream created successfully. StreamId: {StreamId}, SellerId: {SellerId}", stream.Id, request.SellerId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<LiveStreamDto>(createdStream);
    }
}

