using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Application.Content.Commands.CreateOrUpdateSEOSettings;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Commands.GenerateProductSEO;

public class GenerateProductSEOCommandHandler(
    IDbContext context,
    IMediator mediator,
    IMapper mapper,
    ILogger<GenerateProductSEOCommandHandler> logger) : IRequestHandler<GenerateProductSEOCommand, SEOSettingsDto>
{

    public async Task<SEOSettingsDto> Handle(GenerateProductSEOCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating SEO for product. ProductId: {ProductId}", request.ProductId);

        var product = await context.Set<ProductEntity>()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null)
        {
            logger.LogWarning("Product not found. ProductId: {ProductId}", request.ProductId);
            throw new NotFoundException("Ürün", request.ProductId);
        }

        var metaTitle = $"{product.Name} - {product.Category?.Name ?? "Product"}";
        var metaDescription = !string.IsNullOrEmpty(product.Description) 
            ? product.Description.Length > 160 
                ? product.Description.Substring(0, 157) + "..." 
                : product.Description
            : $"Buy {product.Name} online. Best price and quality guaranteed.";

        var command = new CreateOrUpdateSEOSettingsCommand(
            PageType: "Product",
            EntityId: request.ProductId,
            MetaTitle: metaTitle,
            MetaDescription: metaDescription,
            MetaKeywords: $"{product.Name}, {product.Category?.Name}, {product.Brand}",
            CanonicalUrl: $"/products/{product.SKU}",
            OgTitle: metaTitle,
            OgDescription: metaDescription,
            OgImageUrl: product.ImageUrl,
            IsIndexed: true,
            FollowLinks: true,
            Priority: 0.8m,
            ChangeFrequency: "weekly");

        return await mediator.Send(command, cancellationToken);
    }
}

