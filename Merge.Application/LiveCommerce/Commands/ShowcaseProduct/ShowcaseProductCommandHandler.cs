using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.LiveCommerce.Commands.ShowcaseProduct;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class ShowcaseProductCommandHandler : IRequestHandler<ShowcaseProductCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ShowcaseProductCommandHandler> _logger;

    public ShowcaseProductCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ShowcaseProductCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(ShowcaseProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Showcasing product. StreamId: {StreamId}, ProductId: {ProductId}", 
            request.StreamId, request.ProductId);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var streamProduct = await _context.Set<LiveStreamProduct>()
            .FirstOrDefaultAsync(p => p.LiveStreamId == request.StreamId && p.ProductId == request.ProductId, cancellationToken);

        if (streamProduct == null)
        {
            _logger.LogWarning("Product not found in stream. StreamId: {StreamId}, ProductId: {ProductId}", 
                request.StreamId, request.ProductId);
            throw new NotFoundException("Yayındaki ürün", Guid.Empty);
        }

        // Unhighlight all products in this stream
        // ✅ PERFORMANCE: Batch update için ToListAsync gerekli (tracking ile)
        var allProducts = await _context.Set<LiveStreamProduct>()
            .Where(p => p.LiveStreamId == request.StreamId)
            .ToListAsync(cancellationToken);

        foreach (var product in allProducts)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            product.Unhighlight();
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        streamProduct.Showcase();

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product showcased successfully. StreamId: {StreamId}, ProductId: {ProductId}", 
            request.StreamId, request.ProductId);
        return Unit.Value;
    }
}

