using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.International.Commands.DeleteCategoryTranslation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class DeleteCategoryTranslationCommandHandler : IRequestHandler<DeleteCategoryTranslationCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteCategoryTranslationCommandHandler> _logger;

    public DeleteCategoryTranslationCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteCategoryTranslationCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteCategoryTranslationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting category translation. TranslationId: {TranslationId}", request.Id);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var translation = await _context.Set<CategoryTranslation>()
            .FirstOrDefaultAsync(ct => ct.Id == request.Id, cancellationToken);

        if (translation == null)
        {
            _logger.LogWarning("Category translation not found for deletion. TranslationId: {TranslationId}", request.Id);
            throw new NotFoundException("Kategori çevirisi", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
        translation.MarkAsDeleted(); // BaseEntity'den geliyor

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category translation deleted successfully. TranslationId: {TranslationId}", translation.Id);
        return Unit.Value;
    }
}

