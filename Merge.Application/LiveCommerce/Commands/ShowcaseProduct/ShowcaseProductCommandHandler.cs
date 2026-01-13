using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.LiveCommerce.Commands.ShowcaseProduct;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class ShowcaseProductCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<ShowcaseProductCommandHandler> logger) : IRequestHandler<ShowcaseProductCommand, Unit>
{
    public async Task<Unit> Handle(ShowcaseProductCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Showcasing product. StreamId: {StreamId}, ProductId: {ProductId}", 
            request.StreamId, request.ProductId);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var streamProduct = await context.Set<LiveStreamProduct>()
            .FirstOrDefaultAsync(p => p.LiveStreamId == request.StreamId && p.ProductId == request.ProductId, cancellationToken);

        if (streamProduct == null)
        {
            logger.LogWarning("Product not found in stream. StreamId: {StreamId}, ProductId: {ProductId}", 
                request.StreamId, request.ProductId);
            throw new NotFoundException("Yayındaki ürün", Guid.Empty);
        }

        // Unhighlight all products in this stream
        // ✅ PERFORMANCE: Batch update için ToListAsync gerekli (tracking ile)
        var allProducts = await context.Set<LiveStreamProduct>()
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
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Product showcased successfully. StreamId: {StreamId}, ProductId: {ProductId}", 
            request.StreamId, request.ProductId);
        return Unit.Value;
    }
}

