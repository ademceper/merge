using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.User.Commands.DeleteAddress;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeleteAddressCommandHandler : IRequestHandler<DeleteAddressCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteAddressCommandHandler> _logger;

    public DeleteAddressCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteAddressCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting address with ID: {AddressId}", request.Id);

        var address = await _context.Set<Address>()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (address == null)
        {
            _logger.LogWarning("Address not found with ID: {AddressId}", request.Id);
            return false;
        }

        // ✅ BOLUM 3.2: IDOR koruması - Kullanıcı sadece kendi adreslerini silebilmeli
        if (request.UserId.HasValue && address.UserId != request.UserId.Value && !request.IsAdminOrManager)
        {
            _logger.LogWarning("Unauthorized delete attempt to address {AddressId} by user {UserId}", 
                request.Id, request.UserId.Value);
            throw new Application.Exceptions.BusinessException("Bu adresi silme yetkiniz bulunmamaktadır.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı (soft delete)
        address.MarkAsDeleted();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address deleted successfully with ID: {AddressId}", request.Id);

        return true;
    }
}
