using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Cart;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.Cart.Queries.GetCartByUserId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
// ✅ ARCHITECTURE: Query handler'da UnitOfWork kullanılmamalı (read-only operation)
public class GetCartByUserIdQueryHandler : IRequestHandler<GetCartByUserIdQuery, CartDto>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCartByUserIdQueryHandler> _logger;

    public GetCartByUserIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetCartByUserIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CartDto> Handle(GetCartByUserIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving cart for user {UserId}", request.UserId);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        // ✅ PERFORMANCE: Removed manual !ci.IsDeleted check (Global Query Filter handles it)
        var cart = await _context.Set<Merge.Domain.Modules.Ordering.Cart>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (cart is null)
        {
            // ✅ ARCHITECTURE: Query handler'da write işlemi YAPILMAMALI!
            // Cart oluşturma işlemi AddItemToCartCommandHandler'da yapılır
            // Burada sadece boş cart DTO döndürülür
            _logger.LogInformation("Cart not found for user {UserId}, returning empty cart", request.UserId);
            
            // ✅ ARCHITECTURE: Boş cart DTO döndür (cart oluşturma işlemi command handler'da yapılmalı)
            return new CartDto(
                Id: Guid.Empty,
                UserId: request.UserId,
                CartItems: Array.Empty<CartItemDto>(),
                TotalAmount: 0
            );
        }

        _logger.LogInformation("Retrieved cart {CartId} with {ItemCount} items for user {UserId}",
            cart.Id, cart.CartItems?.Count ?? 0, request.UserId);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<CartDto>(cart);
    }
}

