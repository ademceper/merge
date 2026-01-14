using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.VerifyStore;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class VerifyStoreCommandHandler : IRequestHandler<VerifyStoreCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VerifyStoreCommandHandler> _logger;

    public VerifyStoreCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<VerifyStoreCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(VerifyStoreCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Verifying store. StoreId: {StoreId}", request.StoreId);

        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == request.StoreId, cancellationToken);

        if (store == null)
        {
            _logger.LogWarning("Store not found. StoreId: {StoreId}", request.StoreId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        store.Verify();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Store verified. StoreId: {StoreId}", request.StoreId);

        return true;
    }
}
