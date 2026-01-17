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
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Cart.Commands.AddItemToCart;

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

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var cart = await context.Set<CartEntity>()
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

            if (cart is null)
            {
                logger.LogInformation("Creating new cart for user {UserId}", request.UserId);
                
                var user = await context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
                
                if (user is null)
                {
                    throw new NotFoundException("Kullanıcı", request.UserId);
                }
                
                cart = CartEntity.Create(request.UserId, user);
                await context.Set<CartEntity>().AddAsync(cart, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            var product = await context.Set<ProductEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
            
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

            var maxQuantity = cartSettings.Value.MaxCartItemQuantity;

            var itemPrice = product.DiscountPrice ?? product.Price;
            var itemPriceMoney = new Money(itemPrice);
            var cartItem = CartItem.Create(
                cart.Id,
                request.ProductId,
                request.Quantity,
                itemPriceMoney);

            cart.AddItem(cartItem, maxQuantity);
            
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var updatedOrNewItem = cart.CartItems.FirstOrDefault(ci => 
                ci.ProductId == request.ProductId && 
                ci.ProductVariantId == cartItem.ProductVariantId &&
                !ci.IsDeleted);

            logger.LogInformation(
                "Item added/updated in cart. UserId: {UserId}, ProductId: {ProductId}, Quantity: {Quantity}, CartItemId: {CartItemId}",
                request.UserId, request.ProductId, updatedOrNewItem?.Quantity ?? request.Quantity, updatedOrNewItem?.Id ?? cartItem.Id);

            var itemId = updatedOrNewItem?.Id ?? cartItem.Id;
            var itemToReturn = await context.Set<CartItem>()
                .AsNoTracking()
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == itemId, cancellationToken);

            if (itemToReturn is null)
            {
                logger.LogError(
                    "Cart item not found after adding. CartItemId: {CartItemId}, ProductId: {ProductId}",
                    updatedOrNewItem?.Id ?? cartItem.Id, request.ProductId);
                throw new InvalidOperationException("Sepet öğesi eklenemedi.");
            }

            return mapper.Map<CartItemDto>(itemToReturn);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error adding item to cart. UserId: {UserId}, ProductId: {ProductId}, Quantity: {Quantity}",
                request.UserId, request.ProductId, request.Quantity);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

