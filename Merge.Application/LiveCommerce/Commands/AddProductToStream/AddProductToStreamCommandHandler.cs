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

namespace Merge.Application.LiveCommerce.Commands.AddProductToStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class AddProductToStreamCommandHandler : IRequestHandler<AddProductToStreamCommand, LiveStreamDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AddProductToStreamCommandHandler> _logger;

    public AddProductToStreamCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AddProductToStreamCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LiveStreamDto> Handle(AddProductToStreamCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding product to stream. StreamId: {StreamId}, ProductId: {ProductId}", 
            request.StreamId, request.ProductId);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var stream = await _context.Set<LiveStream>()
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream == null)
        {
            _logger.LogWarning("Stream not found. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        var existing = await _context.Set<LiveStreamProduct>()
            .FirstOrDefaultAsync(p => p.LiveStreamId == request.StreamId && p.ProductId == request.ProductId, cancellationToken);

        if (existing != null)
        {
            _logger.LogWarning("Product already added to stream. StreamId: {StreamId}, ProductId: {ProductId}", 
                request.StreamId, request.ProductId);
            throw new BusinessException("Ürün zaten yayına eklenmiş.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var streamProduct = LiveStreamProduct.Create(
            request.StreamId,
            request.ProductId,
            request.DisplayOrder,
            request.SpecialPrice,
            request.ShowcaseNotes);

        await _context.Set<LiveStreamProduct>().AddAsync(streamProduct, cancellationToken);
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
            _logger.LogWarning("Stream not found after adding product. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        _logger.LogInformation("Product added to stream successfully. StreamId: {StreamId}, ProductId: {ProductId}", 
            request.StreamId, request.ProductId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<LiveStreamDto>(updatedStream);
    }
}

