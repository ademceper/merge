using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.DeleteVolumeDiscount;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeleteVolumeDiscountCommandHandler : IRequestHandler<DeleteVolumeDiscountCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteVolumeDiscountCommandHandler> _logger;

    public DeleteVolumeDiscountCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteVolumeDiscountCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteVolumeDiscountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting volume discount. VolumeDiscountId: {VolumeDiscountId}", request.Id);

        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var discount = await _context.Set<VolumeDiscount>()
            .FirstOrDefaultAsync(vd => vd.Id == request.Id, cancellationToken);

        if (discount == null)
        {
            _logger.LogWarning("Volume discount not found with Id: {VolumeDiscountId}", request.Id);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Soft delete (IsDeleted flag)
        // ✅ BOLUM 1.1: Rich Domain Model - Soft delete (IsDeleted flag)
        discount.MarkAsDeleted();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Volume discount deleted successfully. VolumeDiscountId: {VolumeDiscountId}", request.Id);
        return true;
    }
}

