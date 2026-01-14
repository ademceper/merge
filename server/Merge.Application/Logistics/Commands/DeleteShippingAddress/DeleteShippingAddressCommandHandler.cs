using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Commands.DeleteShippingAddress;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class DeleteShippingAddressCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteShippingAddressCommandHandler> logger) : IRequestHandler<DeleteShippingAddressCommand, Unit>
{

    public async Task<Unit> Handle(DeleteShippingAddressCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting shipping address. AddressId: {AddressId}", request.Id);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var address = await context.Set<ShippingAddress>()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (address == null)
        {
            logger.LogWarning("Shipping address not found for deletion. AddressId: {AddressId}", request.Id);
            throw new NotFoundException("Kargo adresi", request.Id);
        }

        // ✅ PERFORMANCE: AsNoTracking - Check if address is used in any orders
        var hasOrders = await context.Set<OrderEntity>()
            .AsNoTracking()
            .AnyAsync(o => o.AddressId == request.Id, cancellationToken);

        if (hasOrders)
        {
            // Soft delete - just mark as inactive
            address.Deactivate();
            address.UnsetAsDefault();
        }
        else
        {
            // Hard delete if no orders
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            address.MarkAsDeleted();
        }

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Shipping address deleted successfully. AddressId: {AddressId}", request.Id);
        return Unit.Value;
    }
}

