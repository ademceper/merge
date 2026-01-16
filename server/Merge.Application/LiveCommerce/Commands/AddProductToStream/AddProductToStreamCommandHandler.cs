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

namespace Merge.Application.LiveCommerce.Commands.AddProductToStream;

public class AddProductToStreamCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<AddProductToStreamCommandHandler> logger) : IRequestHandler<AddProductToStreamCommand, LiveStreamDto>
{
    public async Task<LiveStreamDto> Handle(AddProductToStreamCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding product to stream. StreamId: {StreamId}, ProductId: {ProductId}", 
            request.StreamId, request.ProductId);

        var stream = await context.Set<LiveStream>()
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream == null)
        {
            logger.LogWarning("Stream not found. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        var existing = await context.Set<LiveStreamProduct>()
            .FirstOrDefaultAsync(p => p.LiveStreamId == request.StreamId && p.ProductId == request.ProductId, cancellationToken);

        if (existing != null)
        {
            logger.LogWarning("Product already added to stream. StreamId: {StreamId}, ProductId: {ProductId}", 
                request.StreamId, request.ProductId);
            throw new BusinessException("Ürün zaten yayına eklenmiş.");
        }

        var streamProduct = LiveStreamProduct.Create(
            request.StreamId,
            request.ProductId,
            request.DisplayOrder,
            request.SpecialPrice,
            request.ShowcaseNotes);

        // Aggregate root üzerinden product ekleme (encapsulation)
        stream.AddProduct(streamProduct);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedStream = await context.Set<LiveStream>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .Include(s => s.Products)
                .ThenInclude(p => p.Product)
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (updatedStream == null)
        {
            logger.LogWarning("Stream not found after adding product. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        logger.LogInformation("Product added to stream successfully. StreamId: {StreamId}, ProductId: {ProductId}", 
            request.StreamId, request.ProductId);

        return mapper.Map<LiveStreamDto>(updatedStream);
    }
}
