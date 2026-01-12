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

        var cartItem = await _context.Set<CartItem>()
            .FirstOrDefaultAsync(ci => ci.Id == request.CartItemId, cancellationToken);
        
        if (cartItem == null)
        {
            _logger.LogWarning("Cart item {CartItemId} not found", request.CartItemId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        // Cart entity'sinin RemoveItem method'unu kullan
        var cart = await _context.Set<Merge.Domain.Modules.Ordering.Cart>()
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.Id == cartItem.CartId, cancellationToken);

        if (cart == null)
        {
            _logger.LogWarning("Cart not found for cart item {CartItemId}", request.CartItemId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
        // ✅ ARCHITECTURE: Domain event'ler entity içinde oluşturuluyor (Cart.RemoveItem() içinde CartItemRemovedEvent)
        cart.RemoveItem(request.CartItemId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Successfully removed item from cart. CartItemId: {CartItemId}, ProductId: {ProductId}",
            request.CartItemId, cartItem.ProductId);

        return true;
    }
}

