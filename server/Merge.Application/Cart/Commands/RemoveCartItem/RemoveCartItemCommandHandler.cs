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

namespace Merge.Application.Cart.Commands.RemoveCartItem;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveCartItemCommandHandler> _logger;

    public RemoveCartItemCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<RemoveCartItemCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing item from cart. CartItemId: {CartItemId}", request.CartItemId);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (Cart + CartItem + Updates)
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var cartItem = await _context.Set<CartItem>()
                .FirstOrDefaultAsync(ci => ci.Id == request.CartItemId, cancellationToken);
            
            // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
            if (cartItem is null)
            {
                _logger.LogWarning("Cart item {CartItemId} not found", request.CartItemId);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return false;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
            // Cart entity'sinin RemoveItem method'unu kullan
            var cart = await _context.Set<Merge.Domain.Modules.Ordering.Cart>()
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == cartItem.CartId, cancellationToken);

            // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
            if (cart is null)
            {
                _logger.LogWarning("Cart not found for cart item {CartItemId}", request.CartItemId);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return false;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
            // ✅ ARCHITECTURE: Domain event'ler entity içinde oluşturuluyor (Cart.RemoveItem() içinde CartItemRemovedEvent)
            cart.RemoveItem(request.CartItemId);
            
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully removed item from cart. CartItemId: {CartItemId}, ProductId: {ProductId}",
                request.CartItemId, cartItem.ProductId);

            return true;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error removing item from cart. CartItemId: {CartItemId}",
                request.CartItemId);
            // ✅ ARCHITECTURE: Hata olursa ROLLBACK - hiçbir şey yazılmaz
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

