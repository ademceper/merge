using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Cart;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.SaveItem;

public class SaveItemCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<SaveItemCommandHandler> logger) : IRequestHandler<SaveItemCommand, SavedCartItemDto>
{

    public async Task<SavedCartItemDto> Handle(SaveItemCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Set<ProductEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
        
        if (product is null || !product.IsActive)
        {
            throw new NotFoundException("Ürün", request.ProductId);
        }

        var existing = await context.Set<SavedCartItem>()
            .FirstOrDefaultAsync(sci => sci.UserId == request.UserId &&
                                      sci.ProductId == request.ProductId, cancellationToken);

        var currentPrice = product.DiscountPrice ?? product.Price;
        var currentPriceMoney = new Money(currentPrice);

        if (existing is not null)
        {
            existing.UpdateQuantity(request.Quantity);
            existing.UpdatePrice(currentPriceMoney);
            existing.UpdateNotes(request.Notes);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            existing = await context.Set<SavedCartItem>()
                .AsNoTracking()
                .Include(sci => sci.Product)
                .FirstOrDefaultAsync(sci => sci.Id == existing.Id, cancellationToken);

            return mapper.Map<SavedCartItemDto>(existing!);
        }

        var savedItem = SavedCartItem.Create(request.UserId, request.ProductId, request.Quantity, currentPriceMoney, request.Notes);

        await context.Set<SavedCartItem>().AddAsync(savedItem, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        savedItem = await context.Set<SavedCartItem>()
            .AsNoTracking()
            .Include(sci => sci.Product)
            .FirstOrDefaultAsync(sci => sci.Id == savedItem.Id, cancellationToken);

        return mapper.Map<SavedCartItemDto>(savedItem!);
    }
}

