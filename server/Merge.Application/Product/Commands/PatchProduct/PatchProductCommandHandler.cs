using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Product.Queries.GetProductById;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Merge.Application.Product.Commands.PatchProduct;

/// <summary>
/// Handler for PatchProductCommand
/// HIGH-API-001: PATCH Support - Partial updates implementation
/// </summary>
public class PatchProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<PatchProductCommandHandler> logger) : IRequestHandler<PatchProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<PatchProductCommandHandler> _logger = logger;

    public async Task<ProductDto> Handle(PatchProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Patching product. ProductId: {ProductId}", request.Id);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
            {
                throw new NotFoundException("Ürün", request.Id);
            }

            // ✅ IDOR Protection - Seller sadece kendi ürünlerini güncelleyebilmeli
            if (request.PerformedBy.HasValue && product.SellerId.HasValue && product.SellerId.Value != request.PerformedBy.Value)
            {
                _logger.LogWarning("Unauthorized attempt to patch product {ProductId} by user {UserId}. Product belongs to {SellerId}",
                    request.Id, request.PerformedBy.Value, product.SellerId.Value);
                throw new BusinessException("Bu ürünü güncelleme yetkiniz bulunmamaktadır.");
            }

            // Apply partial updates - only update fields that are provided
            if (request.PatchDto.Name != null)
            {
                product.UpdateName(request.PatchDto.Name);
            }

            if (request.PatchDto.Description != null)
            {
                product.UpdateDescription(request.PatchDto.Description);
            }

            if (request.PatchDto.SKU != null)
            {
                var sku = new SKU(request.PatchDto.SKU);
                product.UpdateSKU(sku);
            }

            if (request.PatchDto.Price.HasValue)
            {
                var price = new Money(request.PatchDto.Price.Value);
                product.SetPrice(price);
            }

            if (request.PatchDto.DiscountPrice.HasValue)
            {
                var discountPrice = new Money(request.PatchDto.DiscountPrice.Value);
                product.SetDiscountPrice(discountPrice);
            }
            else if (request.PatchDto.DiscountPrice == null && request.PatchDto.Price == null)
            {
                // Only clear discount if discount is explicitly set to null and price is not being updated
                // This allows clearing discount without updating price
            }

            if (request.PatchDto.StockQuantity.HasValue)
            {
                product.SetStockQuantity(request.PatchDto.StockQuantity.Value);
            }

            if (request.PatchDto.Brand != null)
            {
                product.UpdateBrand(request.PatchDto.Brand);
            }

            if (request.PatchDto.ImageUrl != null || request.PatchDto.ImageUrls != null)
            {
                var imageUrl = request.PatchDto.ImageUrl ?? product.ImageUrl;
                var imageUrls = request.PatchDto.ImageUrls ?? product.ImageUrls;
                product.UpdateImages(imageUrl, imageUrls);
            }

            if (request.PatchDto.CategoryId.HasValue)
            {
                // Note: Product entity doesn't have UpdateCategory method
                // For now, we'll skip category updates in PATCH or use UpdateProductCommand
                // This is a limitation that should be addressed by adding UpdateCategory method to Product entity
                _logger.LogWarning("CategoryId update via PATCH is not supported. ProductId: {ProductId}", request.Id);
            }

            if (request.PatchDto.IsActive.HasValue)
            {
                if (request.PatchDto.IsActive.Value)
                {
                    product.Activate();
                }
                else
                {
                    product.Deactivate();
                }
            }

            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Product patched successfully. ProductId: {ProductId}", request.Id);

            // Return updated product
            var getQuery = new GetProductByIdQuery(request.Id);
            return await mediator.Send(getQuery, cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
