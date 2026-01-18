using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.ValueObjects;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.AddProductToFlashSale;

public class AddProductToFlashSaleCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<AddProductToFlashSaleCommandHandler> logger) : IRequestHandler<AddProductToFlashSaleCommand, bool>
{
    public async Task<bool> Handle(AddProductToFlashSaleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding product to flash sale. FlashSaleId: {FlashSaleId}, ProductId: {ProductId}", 
            request.FlashSaleId, request.ProductId);

        var flashSale = await context.Set<FlashSale>()
            .FirstOrDefaultAsync(fs => fs.Id == request.FlashSaleId, cancellationToken);

        if (flashSale == null)
        {
            logger.LogWarning("FlashSale not found. FlashSaleId: {FlashSaleId}", request.FlashSaleId);
            throw new NotFoundException("Flash Sale", request.FlashSaleId);
        }

        var product = await context.Set<ProductEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null)
        {
            logger.LogWarning("Product not found. ProductId: {ProductId}", request.ProductId);
            throw new NotFoundException("Ürün", request.ProductId);
        }

        // Check if product already exists in flash sale
        var existingProduct = await context.Set<FlashSaleProduct>()
            .AsNoTracking()
            .AnyAsync(fsp => fsp.FlashSaleId == request.FlashSaleId && fsp.ProductId == request.ProductId, cancellationToken);

        if (existingProduct)
        {
            logger.LogWarning("Product already exists in flash sale. FlashSaleId: {FlashSaleId}, ProductId: {ProductId}", 
                request.FlashSaleId, request.ProductId);
            throw new BusinessException("Bu ürün flash sale'de zaten mevcut.");
        }

        var salePrice = new Money(request.SalePrice);
        var flashSaleProduct = FlashSaleProduct.Create(
            request.FlashSaleId,
            request.ProductId,
            salePrice,
            request.StockLimit,
            request.SortOrder);

        await context.Set<FlashSaleProduct>().AddAsync(flashSaleProduct, cancellationToken);
        
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Product added to flash sale successfully. FlashSaleId: {FlashSaleId}, ProductId: {ProductId}", 
            request.FlashSaleId, request.ProductId);

        return true;
    }
}
