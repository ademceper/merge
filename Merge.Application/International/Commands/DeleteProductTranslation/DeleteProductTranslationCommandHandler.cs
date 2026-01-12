using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.DeleteProductTranslation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class DeleteProductTranslationCommandHandler : IRequestHandler<DeleteProductTranslationCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteProductTranslationCommandHandler> _logger;

    public DeleteProductTranslationCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteProductTranslationCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteProductTranslationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting product translation. TranslationId: {TranslationId}", request.Id);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var translation = await _context.Set<ProductTranslation>()
            .FirstOrDefaultAsync(pt => pt.Id == request.Id, cancellationToken);

        if (translation == null)
        {
            _logger.LogWarning("Product translation not found for deletion. TranslationId: {TranslationId}", request.Id);
            throw new NotFoundException("Ürün çevirisi", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
        translation.MarkAsDeleted();

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product translation deleted successfully. TranslationId: {TranslationId}", translation.Id);
        return Unit.Value;
    }
}

