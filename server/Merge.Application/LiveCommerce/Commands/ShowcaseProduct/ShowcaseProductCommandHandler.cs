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

public class ShowcaseProductCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<ShowcaseProductCommandHandler> logger) : IRequestHandler<ShowcaseProductCommand, Unit>
{
    public async Task<Unit> Handle(ShowcaseProductCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Showcasing product. StreamId: {StreamId}, ProductId: {ProductId}", 
            request.StreamId, request.ProductId);

        var streamProduct = await context.Set<LiveStreamProduct>()
            .FirstOrDefaultAsync(p => p.LiveStreamId == request.StreamId && p.ProductId == request.ProductId, cancellationToken);

        if (streamProduct == null)
        {
            logger.LogWarning("Product not found in stream. StreamId: {StreamId}, ProductId: {ProductId}", 
                request.StreamId, request.ProductId);
            throw new NotFoundException("Yayındaki ürün", Guid.Empty);
        }

        // Unhighlight all products in this stream
       
        var allProducts = await context.Set<LiveStreamProduct>()
            .Where(p => p.LiveStreamId == request.StreamId)
            .ToListAsync(cancellationToken);

        foreach (var product in allProducts)
        {
            product.Unhighlight();
        }

        streamProduct.Showcase();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Product showcased successfully. StreamId: {StreamId}, ProductId: {ProductId}", 
            request.StreamId, request.ProductId);
        return Unit.Value;
    }
}
