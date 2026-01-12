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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class AddProductToFlashSaleCommandHandler : IRequestHandler<AddProductToFlashSaleCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddProductToFlashSaleCommandHandler> _logger;

    public AddProductToFlashSaleCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<AddProductToFlashSaleCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(AddProductToFlashSaleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding product to flash sale. FlashSaleId: {FlashSaleId}, ProductId: {ProductId}", 
            request.FlashSaleId, request.ProductId);

        var flashSale = await _context.Set<FlashSale>()
            .FirstOrDefaultAsync(fs => fs.Id == request.FlashSaleId, cancellationToken);

        if (flashSale == null)
        {
            _logger.LogWarning("FlashSale not found. FlashSaleId: {FlashSaleId}", request.FlashSaleId);
            throw new NotFoundException("Flash Sale", request.FlashSaleId);
        }

        var product = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Product not found. ProductId: {ProductId}", request.ProductId);
            throw new NotFoundException("Ürün", request.ProductId);
        }

        // Check if product already exists in flash sale
        var existingProduct = await _context.Set<FlashSaleProduct>()
            .AsNoTracking()
            .AnyAsync(fsp => fsp.FlashSaleId == request.FlashSaleId && fsp.ProductId == request.ProductId, cancellationToken);

        if (existingProduct)
        {
            _logger.LogWarning("Product already exists in flash sale. FlashSaleId: {FlashSaleId}, ProductId: {ProductId}", 
                request.FlashSaleId, request.ProductId);
            throw new BusinessException("Bu ürün flash sale'de zaten mevcut.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var salePrice = new Money(request.SalePrice);
        var flashSaleProduct = FlashSaleProduct.Create(
            request.FlashSaleId,
            request.ProductId,
            salePrice,
            request.StockLimit,
            request.SortOrder);

        await _context.Set<FlashSaleProduct>().AddAsync(flashSaleProduct, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product added to flash sale successfully. FlashSaleId: {FlashSaleId}, ProductId: {ProductId}", 
            request.FlashSaleId, request.ProductId);

        return true;
    }
}
