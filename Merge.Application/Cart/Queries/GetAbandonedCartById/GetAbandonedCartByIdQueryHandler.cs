using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Cart;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using AutoMapper;

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
        var cart = await _context.Set<Merge.Domain.Entities.Cart>()
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

        var itemsDto = _mapper.Map<IEnumerable<CartItemDto>>(items).ToList();

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var dto = _mapper.Map<AbandonedCartDto>(cart);
        dto.CartId = cart.Id;
        dto.ItemCount = itemCount;
        dto.TotalValue = totalValue;
        dto.LastModified = cart.UpdatedAt ?? cart.CreatedAt;
        dto.HoursSinceAbandonment = cart.UpdatedAt.HasValue 
            ? (int)(DateTime.UtcNow - cart.UpdatedAt.Value).TotalHours 
            : (int)(DateTime.UtcNow - cart.CreatedAt).TotalHours;
        dto.Items = itemsDto;
        dto.HasReceivedEmail = hasReceivedEmail;
        dto.EmailsSentCount = emailsSentCount;
        dto.LastEmailSent = lastEmailSent;

        return dto;
    }
}

