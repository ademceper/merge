using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Cart.Commands.AddItemToCart;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.MoveToCart;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class MoveToCartCommandHandler : IRequestHandler<MoveToCartCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<MoveToCartCommandHandler> _logger;

    public MoveToCartCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<MoveToCartCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<bool> Handle(MoveToCartCommand request, CancellationToken cancellationToken)
    {
        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (Cart + SavedCartItem delete)
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // ✅ PERFORMANCE: Removed manual !sci.IsDeleted check (Global Query Filter handles it)
            var item = await _context.Set<SavedCartItem>()
                .Include(sci => sci.Product)
                .FirstOrDefaultAsync(sci => sci.Id == request.ItemId &&
                                          sci.UserId == request.UserId, cancellationToken);

            // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
            if (item is null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return false;
            }

            // ✅ BOLUM 2.0: MediatR + CQRS pattern - AddItemToCartCommand dispatch
            var addItemCommand = new AddItemToCartCommand(request.UserId, item.ProductId, item.Quantity);
            await _mediator.Send(addItemCommand, cancellationToken);
            
            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
            item.MarkAsDeleted();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "SavedCartItem sepete tasima hatasi. UserId: {UserId}, ItemId: {ItemId}",
                request.UserId, request.ItemId);
            // ✅ ARCHITECTURE: Hata olursa ROLLBACK - hiçbir şey yazılmaz
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

