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
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.AddItemToCart;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class AddItemToCartCommandHandler : IRequestHandler<AddItemToCartCommand, CartItemDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AddItemToCartCommandHandler> _logger;
    private readonly CartSettings _cartSettings;

    public AddItemToCartCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AddItemToCartCommandHandler> logger,
        IOptions<CartSettings> cartSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cartSettings = cartSettings.Value;
    }

    public async Task<CartItemDto> Handle(AddItemToCartCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Adding item to cart. UserId: {UserId}, ProductId: {ProductId}, Quantity: {Quantity}",
            request.UserId, request.ProductId, request.Quantity);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (Cart + CartItem + Updates)
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // ✅ PERFORMANCE: Removed manual !ci.IsDeleted check (Global Query Filter)
            var cart = await _context.Set<CartEntity>()
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

            // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
            if (cart is null)
            {
                _logger.LogInformation("Creating new cart for user {UserId}", request.UserId);
                // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullanımı
                cart = CartEntity.Create(request.UserId);
                await _context.Set<CartEntity>().AddAsync(cart, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // ✅ PERFORMANCE: AsNoTracking for read-only product query
            var product = await _context.Set<Merge.Domain.Modules.Catalog.Product>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
            
            // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
            if (product is null)
            {
                _logger.LogWarning("Product {ProductId} not found for user {UserId}", request.ProductId, request.UserId);
                throw new NotFoundException("Ürün", request.ProductId);
            }

            if (!product.IsActive)
            {
                _logger.LogWarning(
                    "Product {ProductId} is inactive for user {UserId}",
                    request.ProductId, request.UserId);
                throw new BusinessException("Ürün aktif değil.");
            }

            if (product.StockQuantity < request.Quantity)
            {
                _logger.LogWarning(
                    "Insufficient stock for product {ProductId}. Available: {Available}, Requested: {Requested}",
                    request.ProductId, product.StockQuantity, request.Quantity);
                throw new BusinessException("Yeterli stok yok.");
            }

            // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration'dan al
            var maxQuantity = _cartSettings.MaxCartItemQuantity;

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
            // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
            // Check if item already exists (same product and variant)
            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);
            
            if (existingItem is not null)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
                existingItem.UpdateQuantity(existingItem.Quantity + request.Quantity, maxQuantity);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation(
                    "Updated cart item quantity. UserId: {UserId}, ProductId: {ProductId}, NewQuantity: {Quantity}",
                    request.UserId, request.ProductId, existingItem.Quantity);

                // ✅ PERFORMANCE: Use single query with Include instead of LoadAsync
                var updatedItem = await _context.Set<CartItem>()
                    .AsNoTracking()
                    .Include(ci => ci.Product)
                    .FirstOrDefaultAsync(ci => ci.Id == existingItem.Id, cancellationToken);

                // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
                return _mapper.Map<CartItemDto>(updatedItem!);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullanımı
            var cartItem = CartItem.Create(
                cart.Id,
                request.ProductId,
                request.Quantity,
                product.DiscountPrice ?? product.Price);

            // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
            cart.AddItem(cartItem, maxQuantity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation(
                "Added new item to cart. UserId: {UserId}, ProductId: {ProductId}, Quantity: {Quantity}, CartItemId: {CartItemId}",
                request.UserId, request.ProductId, request.Quantity, cartItem.Id);

            // ✅ PERFORMANCE: Use single query with Include instead of LoadAsync
            var newItem = await _context.Set<CartItem>()
                .AsNoTracking()
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == cartItem.Id, cancellationToken);

            // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
            return _mapper.Map<CartItemDto>(newItem!);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error adding item to cart. UserId: {UserId}, ProductId: {ProductId}, Quantity: {Quantity}",
                request.UserId, request.ProductId, request.Quantity);
            // ✅ ARCHITECTURE: Hata olursa ROLLBACK - hiçbir şey yazılmaz
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

