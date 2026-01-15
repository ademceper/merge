using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Cart;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using AutoMapper;
using CartEntity = Merge.Domain.Modules.Ordering.Cart;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.AddItemToCart;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class AddItemToCartCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<AddItemToCartCommandHandler> logger,
    IOptions<CartSettings> cartSettings) : IRequestHandler<AddItemToCartCommand, CartItemDto>
{

    public async Task<CartItemDto> Handle(AddItemToCartCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Adding item to cart. UserId: {UserId}, ProductId: {ProductId}, Quantity: {Quantity}",
            request.UserId, request.ProductId, request.Quantity);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (Cart + CartItem + Updates)
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // ✅ PERFORMANCE: Removed manual !ci.IsDeleted check (Global Query Filter)
            var cart = await context.Set<CartEntity>()
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

            // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
            if (cart is null)
            {
                logger.LogInformation("Creating new cart for user {UserId}", request.UserId);
                
                // ✅ BOLUM 1.1: Rich Domain Model - User entity'yi yükle (Cart.Create için gerekli)
                // ✅ User entity'si BaseEntity'den türemediği için IDbContext.Users property'si kullanılıyor
                var user = await context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
                
                if (user is null)
                {
                    throw new NotFoundException("Kullanıcı", request.UserId);
                }
                
                // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullanımı
                cart = CartEntity.Create(request.UserId, user);
                await context.Set<CartEntity>().AddAsync(cart, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // ✅ PERFORMANCE: AsNoTracking for read-only product query
            var product = await context.Set<Merge.Domain.Modules.Catalog.Product>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
            
            // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
            if (product is null)
            {
                logger.LogWarning("Product {ProductId} not found for user {UserId}", request.ProductId, request.UserId);
                throw new NotFoundException("Ürün", request.ProductId);
            }

            if (!product.IsActive)
            {
                logger.LogWarning(
                    "Product {ProductId} is inactive for user {UserId}",
                    request.ProductId, request.UserId);
                throw new BusinessException("Ürün aktif değil.");
            }

            if (product.StockQuantity < request.Quantity)
            {
                logger.LogWarning(
                    "Insufficient stock for product {ProductId}. Available: {Available}, Requested: {Requested}",
                    request.ProductId, product.StockQuantity, request.Quantity);
                throw new BusinessException("Yeterli stok yok.");
            }

            // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration'dan al
            var maxQuantity = cartSettings.Value.MaxCartItemQuantity;

            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullanımı
            // ✅ BOLUM 1.3: Value Objects - Money value object kullanımı
            var itemPrice = product.DiscountPrice ?? product.Price;
            var itemPriceMoney = new Merge.Domain.ValueObjects.Money(itemPrice);
            var cartItem = CartItem.Create(
                cart.Id,
                request.ProductId,
                request.Quantity,
                itemPriceMoney);

            // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
            // Cart.AddItem() method'u mevcut item varsa otomatik olarak quantity günceller ve uygun domain event yayınlar
            // ✅ ARCHITECTURE: Domain event'ler Cart.AddItem() içinde otomatik olarak oluşturulur
            cart.AddItem(cartItem, maxQuantity);
            
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
            // Cart.AddItem() sonrası güncellenen veya yeni eklenen item'ı bul
            var updatedOrNewItem = cart.CartItems.FirstOrDefault(ci => 
                ci.ProductId == request.ProductId && 
                ci.ProductVariantId == cartItem.ProductVariantId &&
                !ci.IsDeleted);

            logger.LogInformation(
                "Item added/updated in cart. UserId: {UserId}, ProductId: {ProductId}, Quantity: {Quantity}, CartItemId: {CartItemId}",
                request.UserId, request.ProductId, updatedOrNewItem?.Quantity ?? request.Quantity, updatedOrNewItem?.Id ?? cartItem.Id);

            // ✅ PERFORMANCE: Use single query with Include instead of LoadAsync
            var itemId = updatedOrNewItem?.Id ?? cartItem.Id;
            var itemToReturn = await context.Set<CartItem>()
                .AsNoTracking()
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == itemId, cancellationToken);

            // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
            if (itemToReturn is null)
            {
                logger.LogError(
                    "Cart item not found after adding. CartItemId: {CartItemId}, ProductId: {ProductId}",
                    updatedOrNewItem?.Id ?? cartItem.Id, request.ProductId);
                throw new InvalidOperationException("Sepet öğesi eklenemedi.");
            }

            // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
            return mapper.Map<CartItemDto>(itemToReturn);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error adding item to cart. UserId: {UserId}, ProductId: {ProductId}, Quantity: {Quantity}",
                request.UserId, request.ProductId, request.Quantity);
            // ✅ ARCHITECTURE: Hata olursa ROLLBACK - hiçbir şey yazılmaz
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

