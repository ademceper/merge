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

namespace Merge.Application.Cart.Queries.GetCartByUserId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetCartByUserIdQueryHandler : IRequestHandler<GetCartByUserIdQuery, CartDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCartByUserIdQueryHandler> _logger;

    public GetCartByUserIdQueryHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetCartByUserIdQueryHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
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
            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullanımı
            var newCart = Merge.Domain.Modules.Ordering.Cart.Create(request.UserId);
            await _context.Set<Merge.Domain.Modules.Ordering.Cart>().AddAsync(newCart, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created new cart for user {UserId}, CartId: {CartId}",
                request.UserId, newCart.Id);

            // Reload with Include for AutoMapper
            // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
            newCart = await _context.Set<Merge.Domain.Modules.Ordering.Cart>()
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Id == newCart.Id, cancellationToken);

            return _mapper.Map<CartDto>(newCart!);
        }

        _logger.LogInformation("Retrieved cart {CartId} with {ItemCount} items for user {UserId}",
            cart.Id, cart.CartItems?.Count ?? 0, request.UserId);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<CartDto>(cart);
    }
}

