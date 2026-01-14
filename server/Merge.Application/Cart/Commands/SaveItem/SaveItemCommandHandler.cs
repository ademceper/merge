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
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.SaveItem;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class SaveItemCommandHandler : IRequestHandler<SaveItemCommand, SavedCartItemDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SaveItemCommandHandler> _logger;

    public SaveItemCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SaveItemCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SavedCartItemDto> Handle(SaveItemCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only product query
        var product = await _context.Set<Merge.Domain.Modules.Catalog.Product>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
        
        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (product is null || !product.IsActive)
        {
            throw new NotFoundException("Ürün", request.ProductId);
        }

        // ✅ PERFORMANCE: Removed manual !sci.IsDeleted check (Global Query Filter handles it)
        var existing = await _context.Set<SavedCartItem>()
            .FirstOrDefaultAsync(sci => sci.UserId == request.UserId &&
                                      sci.ProductId == request.ProductId, cancellationToken);

        var currentPrice = product.DiscountPrice ?? product.Price;
        var currentPriceMoney = new Merge.Domain.ValueObjects.Money(currentPrice);

        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (existing is not null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
            existing.UpdateQuantity(request.Quantity);
            existing.UpdatePrice(currentPriceMoney);
            existing.UpdateNotes(request.Notes);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ ARCHITECTURE: Reload with Include for AutoMapper
            existing = await _context.Set<SavedCartItem>()
                .AsNoTracking()
                .Include(sci => sci.Product)
                .FirstOrDefaultAsync(sci => sci.Id == existing.Id, cancellationToken);

            // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
            return _mapper.Map<SavedCartItemDto>(existing!);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullanımı
        // ✅ BOLUM 1.3: Value Objects - Money value object kullanımı
        var savedItem = SavedCartItem.Create(request.UserId, request.ProductId, request.Quantity, currentPriceMoney, request.Notes);

        await _context.Set<SavedCartItem>().AddAsync(savedItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: Reload with Include for AutoMapper
        savedItem = await _context.Set<SavedCartItem>()
            .AsNoTracking()
            .Include(sci => sci.Product)
            .FirstOrDefaultAsync(sci => sci.Id == savedItem.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<SavedCartItemDto>(savedItem!);
    }
}

