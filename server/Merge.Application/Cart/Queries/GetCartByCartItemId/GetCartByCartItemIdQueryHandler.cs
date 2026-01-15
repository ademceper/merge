using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Cart;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Queries.GetCartByCartItemId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetCartByCartItemIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetCartByCartItemIdQueryHandler> logger) : IRequestHandler<GetCartByCartItemIdQuery, CartDto?>
{

    public async Task<CartDto?> Handle(GetCartByCartItemIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var cartItem = await context.Set<CartItem>()
            .AsNoTracking()
            .Include(ci => ci.Cart)
            .FirstOrDefaultAsync(ci => ci.Id == request.CartItemId, cancellationToken);

        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (cartItem is null || cartItem.Cart is null)
        {
            return null;
        }

        // Load cart with items for mapping
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var cart = await context.Set<Merge.Domain.Modules.Ordering.Cart>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.Id == cartItem.Cart.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        return cart is not null ? mapper.Map<CartDto>(cart) : null;
    }
}

