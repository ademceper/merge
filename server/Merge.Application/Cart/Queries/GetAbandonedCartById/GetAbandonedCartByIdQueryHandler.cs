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
using CartEntity = Merge.Domain.Modules.Ordering.Cart;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Queries.GetAbandonedCartById;

public class GetAbandonedCartByIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetAbandonedCartByIdQueryHandler> logger) : IRequestHandler<GetAbandonedCartByIdQuery, AbandonedCartDto?>
{

    public async Task<AbandonedCartDto?> Handle(GetAbandonedCartByIdQuery request, CancellationToken cancellationToken)
    {

        var cart = await context.Set<CartEntity>()
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.Id == request.CartId, cancellationToken);

        if (cart is null)
        {
            return null;
        }

        var emailsSentCount = await context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => e.CartId == request.CartId)
            .CountAsync(cancellationToken);

        var hasReceivedEmail = emailsSentCount > 0;

        var lastEmailSent = await context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => e.CartId == request.CartId)
            .OrderByDescending(e => e.SentAt)
            .Select(e => (DateTime?)e.SentAt)
            .FirstOrDefaultAsync(cancellationToken);

        var itemCount = await context.Set<CartItem>()
            .AsNoTracking()
            .CountAsync(ci => ci.CartId == request.CartId, cancellationToken);

        var totalValue = await context.Set<CartItem>()
            .AsNoTracking()
            .Where(ci => ci.CartId == request.CartId)
            .SumAsync(ci => ci.Price * ci.Quantity, cancellationToken);

        var items = await context.Set<CartItem>()
            .AsNoTracking()
            .Include(ci => ci.Product)
            .Where(ci => ci.CartId == request.CartId)
            .ToListAsync(cancellationToken);

        var itemsDto = mapper.Map<IEnumerable<CartItemDto>>(items).ToList().AsReadOnly();

        var userEmail = cart.User?.Email ?? string.Empty;
        var userName = cart.User is not null ? $"{cart.User.FirstName} {cart.User.LastName}" : string.Empty;
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

