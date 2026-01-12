using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.RemoveSavedItem;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class RemoveSavedItemCommandHandler : IRequestHandler<RemoveSavedItemCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveSavedItemCommandHandler> _logger;

    public RemoveSavedItemCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<RemoveSavedItemCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(RemoveSavedItemCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !sci.IsDeleted check (Global Query Filter handles it)
        var item = await _context.Set<SavedCartItem>()
            .FirstOrDefaultAsync(sci => sci.Id == request.ItemId &&
                                      sci.UserId == request.UserId, cancellationToken);

        if (item == null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        item.MarkAsDeleted();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}

