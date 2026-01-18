using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using ProductBundle = Merge.Domain.Modules.Catalog.ProductBundle;
using BundleItem = Merge.Domain.Modules.Catalog.BundleItem;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IBundleRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Catalog.ProductBundle>;
using IBundleItemRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Catalog.BundleItem>;

namespace Merge.Application.Product.Commands.CreateProductBundle;

public class CreateProductBundleCommandHandler(IBundleRepository bundleRepository, IBundleItemRepository bundleItemRepository, IDbContext context, IUnitOfWork unitOfWork, ICacheService cache, IMapper mapper, ILogger<CreateProductBundleCommandHandler> logger) : IRequestHandler<CreateProductBundleCommand, ProductBundleDto>
{

    private const string CACHE_KEY_BUNDLE_BY_ID = "bundle_";
    private const string CACHE_KEY_ALL_BUNDLES = "bundles_all";
    private const string CACHE_KEY_ACTIVE_BUNDLES = "bundles_active";

    public async Task<ProductBundleDto> Handle(CreateProductBundleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating product bundle. Name: {Name}, ProductCount: {ProductCount}",
            request.Name, request.Products.Count);

        if (!request.Products.Any())
        {
            throw new ValidationException("Paket en az bir ürün içermelidir.");
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var productIds = request.Products.Select(p => p.ProductId).ToList();
            var products = await context.Set<ProductEntity>()
                .AsNoTracking()
                .Where(p => productIds.Contains(p.Id) && p.IsActive)
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            // Validate all products exist
            foreach (var productDto in request.Products)
            {
                if (!products.ContainsKey(productDto.ProductId))
                {
                    throw new NotFoundException("Ürün", productDto.ProductId);
                }
            }

            // Calculate original total price
            decimal originalTotal = request.Products.Sum(productDto =>
                (products[productDto.ProductId].DiscountPrice ?? products[productDto.ProductId].Price) * productDto.Quantity);

            var bundle = ProductBundle.Create(
                request.Name,
                request.Description,
                request.BundlePrice,
                originalTotal,
                request.ImageUrl,
                request.StartDate,
                request.EndDate);

            foreach (var productDto in request.Products)
            {
                bundle.AddItem(productDto.ProductId, productDto.Quantity, productDto.SortOrder);
            }

            bundle = await bundleRepository.AddAsync(bundle, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var reloadedBundle = await context.Set<ProductBundle>()
                .AsNoTracking()
                .Include(b => b.BundleItems)
                    .ThenInclude(bi => bi.Product)
                .FirstOrDefaultAsync(b => b.Id == bundle.Id, cancellationToken);

            if (reloadedBundle is null)
            {
                logger.LogWarning("Product bundle {BundleId} not found after creation", bundle.Id);
                throw new NotFoundException("Paket", bundle.Id);
            }

            await cache.RemoveAsync(CACHE_KEY_ALL_BUNDLES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ACTIVE_BUNDLES, cancellationToken);

            logger.LogInformation("Product bundle created successfully. BundleId: {BundleId}, Name: {Name}, BundlePrice: {BundlePrice}",
                bundle.Id, request.Name, request.BundlePrice);

            return mapper.Map<ProductBundleDto>(reloadedBundle);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating product bundle. Name: {Name}", request.Name);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
