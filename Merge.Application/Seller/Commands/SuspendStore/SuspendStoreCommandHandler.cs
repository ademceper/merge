using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Seller.Commands.SuspendStore;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class SuspendStoreCommandHandler : IRequestHandler<SuspendStoreCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SuspendStoreCommandHandler> _logger;

    public SuspendStoreCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<SuspendStoreCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(SuspendStoreCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Suspending store. StoreId: {StoreId}, Reason: {Reason}",
            request.StoreId, request.Reason);

        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == request.StoreId, cancellationToken);

        if (store == null)
        {
            _logger.LogWarning("Store not found. StoreId: {StoreId}", request.StoreId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        store.Suspend(request.Reason);

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Store suspended. StoreId: {StoreId}", request.StoreId);

        return true;
    }
}
