using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.RemoveProductFromFlashSale;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class RemoveProductFromFlashSaleCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<RemoveProductFromFlashSaleCommandHandler> logger) : IRequestHandler<RemoveProductFromFlashSaleCommand, bool>
{
    public async Task<bool> Handle(RemoveProductFromFlashSaleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Removing product from flash sale. FlashSaleId: {FlashSaleId}, ProductId: {ProductId}", 
            request.FlashSaleId, request.ProductId);

        var flashSaleProduct = await context.Set<FlashSaleProduct>()
            .FirstOrDefaultAsync(fsp => fsp.FlashSaleId == request.FlashSaleId && fsp.ProductId == request.ProductId, cancellationToken);

        if (flashSaleProduct == null)
        {
            logger.LogWarning("FlashSaleProduct not found. FlashSaleId: {FlashSaleId}, ProductId: {ProductId}", 
                request.FlashSaleId, request.ProductId);
            throw new NotFoundException("Flash Sale Product", Guid.Empty);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı (soft delete)
        flashSaleProduct.MarkAsDeleted();
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Product removed from flash sale successfully. FlashSaleId: {FlashSaleId}, ProductId: {ProductId}", 
            request.FlashSaleId, request.ProductId);

        return true;
    }
}
