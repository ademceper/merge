using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.B2B.Commands.UpdateVolumeDiscount;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class UpdateVolumeDiscountCommandHandler : IRequestHandler<UpdateVolumeDiscountCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateVolumeDiscountCommandHandler> _logger;

    public UpdateVolumeDiscountCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UpdateVolumeDiscountCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateVolumeDiscountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating volume discount. VolumeDiscountId: {VolumeDiscountId}", request.Id);

        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, handler'da tekrar validation gereksiz

        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var discount = await _context.Set<VolumeDiscount>()
            .FirstOrDefaultAsync(vd => vd.Id == request.Id, cancellationToken);

        if (discount == null)
        {
            _logger.LogWarning("Volume discount not found with Id: {VolumeDiscountId}", request.Id);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
        discount.UpdateQuantityRange(request.Dto.MinQuantity, request.Dto.MaxQuantity);
        discount.UpdateDiscount(request.Dto.DiscountPercentage, request.Dto.FixedDiscountAmount);
        discount.UpdateDates(request.Dto.StartDate, request.Dto.EndDate);
        if (request.Dto.IsActive)
            discount.Activate();
        else
            discount.Deactivate();
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Volume discount updated successfully. VolumeDiscountId: {VolumeDiscountId}", request.Id);
        return true;
    }
}

