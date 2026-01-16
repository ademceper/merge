using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Cart;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using CartEntity = Merge.Domain.Modules.Ordering.Cart;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.Cart.Queries.GetCartByUserId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
// ✅ ARCHITECTURE: Query handler'da UnitOfWork kullanılmamalı (read-only operation)
public class GetCartByUserIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetCartByUserIdQueryHandler> logger) : IRequestHandler<GetCartByUserIdQuery, CartDto>
{

    public async Task<CartDto> Handle(GetCartByUserIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving cart for user {UserId}", request.UserId);

        var cart = await context.Set<CartEntity>()
            .AsNoTracking()
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (cart is null)
        {
            // ✅ ARCHITECTURE: Query handler'da write işlemi YAPILMAMALI!
            // Cart oluşturma işlemi AddItemToCartCommandHandler'da yapılır
            // Burada sadece boş cart DTO döndürülür
            logger.LogInformation("Cart not found for user {UserId}, returning empty cart", request.UserId);
            
            // ✅ ARCHITECTURE: Boş cart DTO döndür (cart oluşturma işlemi command handler'da yapılmalı)
            return new CartDto(
                Id: Guid.Empty,
                UserId: request.UserId,
                CartItems: Array.Empty<CartItemDto>(),
                TotalAmount: 0
            );
        }

        logger.LogInformation("Retrieved cart {CartId} with {ItemCount} items for user {UserId}",
            cart.Id, cart.CartItems?.Count ?? 0, request.UserId);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return mapper.Map<CartDto>(cart);
    }
}

