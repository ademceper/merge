using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using Merge.Domain.SharedKernel.DomainEvents;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.ClearCart;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ClearCartCommandHandler> _logger;

    public ClearCartCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ClearCartCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (Cart + CartItems + Updates)
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // ✅ PERFORMANCE: Removed manual !ci.IsDeleted check (Global Query Filter)
            var cart = await _context.Set<Merge.Domain.Modules.Ordering.Cart>()
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

            // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
            if (cart is null)
            {
                _logger.LogWarning("Cart not found for user {UserId}", request.UserId);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return false;
            }

            var itemCount = cart.CartItems.Count(ci => !ci.IsDeleted);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
            // ✅ ARCHITECTURE: Domain event'ler entity içinde oluşturuluyor (Cart.Clear() içinde CartClearedEvent)
            cart.Clear();
            
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation(
                "Cleared cart. UserId: {UserId}, ItemsRemoved: {Count}",
                request.UserId, itemCount);

            return true;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error clearing cart. UserId: {UserId}",
                request.UserId);
            // ✅ ARCHITECTURE: Hata olursa ROLLBACK - hiçbir şey yazılmaz
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

