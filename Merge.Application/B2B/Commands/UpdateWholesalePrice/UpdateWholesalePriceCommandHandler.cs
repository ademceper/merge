using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.B2B.Commands.UpdateWholesalePrice;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class UpdateWholesalePriceCommandHandler : IRequestHandler<UpdateWholesalePriceCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateWholesalePriceCommandHandler> _logger;

    public UpdateWholesalePriceCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UpdateWholesalePriceCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateWholesalePriceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating wholesale price. WholesalePriceId: {WholesalePriceId}", request.Id);

        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, handler'da tekrar validation gereksiz

        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var price = await _context.Set<WholesalePrice>()
            .FirstOrDefaultAsync(wp => wp.Id == request.Id, cancellationToken);

        if (price == null)
        {
            _logger.LogWarning("Wholesale price not found with Id: {WholesalePriceId}", request.Id);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
        price.UpdateQuantityRange(request.Dto.MinQuantity, request.Dto.MaxQuantity);
        price.UpdatePrice(request.Dto.Price);
        price.UpdateDates(request.Dto.StartDate, request.Dto.EndDate);
        if (request.Dto.IsActive)
            price.Activate();
        else
            price.Deactivate();
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Wholesale price updated successfully. WholesalePriceId: {WholesalePriceId}", request.Id);
        return true;
    }
}

