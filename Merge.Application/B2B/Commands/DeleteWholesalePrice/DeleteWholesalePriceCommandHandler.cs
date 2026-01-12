using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.DeleteWholesalePrice;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeleteWholesalePriceCommandHandler : IRequestHandler<DeleteWholesalePriceCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteWholesalePriceCommandHandler> _logger;

    public DeleteWholesalePriceCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteWholesalePriceCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteWholesalePriceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting wholesale price. WholesalePriceId: {WholesalePriceId}", request.Id);

        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var price = await _context.Set<WholesalePrice>()
            .FirstOrDefaultAsync(wp => wp.Id == request.Id, cancellationToken);

        if (price == null)
        {
            _logger.LogWarning("Wholesale price not found with Id: {WholesalePriceId}", request.Id);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Soft delete (IsDeleted flag)
        price.IsDeleted = true;
        price.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Wholesale price deleted successfully. WholesalePriceId: {WholesalePriceId}", request.Id);
        return true;
    }
}

