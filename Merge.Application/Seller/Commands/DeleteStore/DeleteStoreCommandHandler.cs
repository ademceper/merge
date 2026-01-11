using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.Seller.Commands.DeleteStore;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteStoreCommandHandler : IRequestHandler<DeleteStoreCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteStoreCommandHandler> _logger;

    public DeleteStoreCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteStoreCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteStoreCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Deleting store. StoreId: {StoreId}", request.StoreId);

        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == request.StoreId, cancellationToken);

        if (store == null)
        {
            _logger.LogWarning("Store not found. StoreId: {StoreId}", request.StoreId);
            return false;
        }

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        // Check if store has products
        var hasProducts = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .AnyAsync(p => p.StoreId == request.StoreId, cancellationToken);

        if (hasProducts)
        {
            _logger.LogWarning("Store deletion failed - Store has products. StoreId: {StoreId}", request.StoreId);
            throw new BusinessException("Ürünleri olan bir mağaza silinemez. Önce ürünleri kaldırın veya transfer edin.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        store.Delete();
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Store deleted. StoreId: {StoreId}", request.StoreId);

        return true;
    }
}
