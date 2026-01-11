using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Seller.Commands.SetPrimaryStore;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class SetPrimaryStoreCommandHandler : IRequestHandler<SetPrimaryStoreCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetPrimaryStoreCommandHandler> _logger;

    public SetPrimaryStoreCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<SetPrimaryStoreCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(SetPrimaryStoreCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Setting primary store. SellerId: {SellerId}, StoreId: {StoreId}",
            request.SellerId, request.StoreId);

        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == request.StoreId && s.SellerId == request.SellerId, cancellationToken);

        if (store == null)
        {
            _logger.LogWarning("Store not found. StoreId: {StoreId}, SellerId: {SellerId}",
                request.StoreId, request.SellerId);
            return false;
        }

        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        // Unset other primary stores
        var existingPrimary = await _context.Set<Store>()
            .Where(s => s.SellerId == request.SellerId && s.IsPrimary && s.Id != request.StoreId)
            .ToListAsync(cancellationToken);

        foreach (var s in existingPrimary)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            s.RemovePrimaryStatus();
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        store.SetAsPrimary();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Primary store set. StoreId: {StoreId}", request.StoreId);

        return true;
    }
}
