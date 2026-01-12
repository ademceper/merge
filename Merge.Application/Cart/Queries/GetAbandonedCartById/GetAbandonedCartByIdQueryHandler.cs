using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Cart;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Queries.GetAbandonedCartById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetAbandonedCartByIdQueryHandler : IRequestHandler<GetAbandonedCartByIdQuery, AbandonedCartDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAbandonedCartByIdQueryHandler> _logger;

    public GetAbandonedCartByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetAbandonedCartByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AbandonedCartDto?> Handle(GetAbandonedCartByIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
        var cart = await _context.Set<Merge.Domain.Modules.Ordering.Cart>()
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.Id == request.CartId, cancellationToken);

        if (cart == null)
        {
            return null;
        }

        // ✅ PERFORMANCE: Database'de Count ve FirstOrDefault yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted check (Global Query Filter handles it)
        var emailsSentCount = await _context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => e.CartId == request.CartId)
            .CountAsync(cancellationToken);

        var hasReceivedEmail = emailsSentCount > 0;

        var lastEmailSent = await _context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => e.CartId == request.CartId)
            .OrderByDescending(e => e.SentAt)
            .Select(e => (DateTime?)e.SentAt)
            .FirstOrDefaultAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de Sum ve Count yap (memory'de işlem YASAK)
        var itemCount = await _context.Set<CartItem>()
            .AsNoTracking()
            .CountAsync(ci => ci.CartId == request.CartId, cancellationToken);

        var totalValue = await _context.Set<CartItem>()
            .AsNoTracking()
            .Where(ci => ci.CartId == request.CartId)
            .SumAsync(ci => ci.Price * ci.Quantity, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var items = await _context.Set<CartItem>()
            .AsNoTracking()
            .Include(ci => ci.Product)
            .Where(ci => ci.CartId == request.CartId)
            .ToListAsync(cancellationToken);

        var itemsDto = _mapper.Map<IEnumerable<CartItemDto>>(items).ToList().AsReadOnly();

        // ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı) - Positional constructor kullanımı
        var userEmail = cart.User?.Email ?? string.Empty;
        var userName = cart.User != null ? $"{cart.User.FirstName} {cart.User.LastName}" : string.Empty;
        var lastModified = cart.UpdatedAt ?? cart.CreatedAt;
        var hoursSinceAbandonment = cart.UpdatedAt.HasValue 
            ? (int)(DateTime.UtcNow - cart.UpdatedAt.Value).TotalHours 
            : (int)(DateTime.UtcNow - cart.CreatedAt).TotalHours;

        var dto = new AbandonedCartDto(
            cart.Id,
            cart.UserId,
            userEmail,
            userName,
            itemCount,
            totalValue,
            lastModified,
            hoursSinceAbandonment,
            itemsDto,
            hasReceivedEmail,
            emailsSentCount,
            lastEmailSent
        );

        return dto;
    }
}

