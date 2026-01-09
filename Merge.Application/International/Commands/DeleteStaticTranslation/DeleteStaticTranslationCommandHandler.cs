using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.International.Commands.DeleteStaticTranslation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class DeleteStaticTranslationCommandHandler : IRequestHandler<DeleteStaticTranslationCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteStaticTranslationCommandHandler> _logger;

    public DeleteStaticTranslationCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteStaticTranslationCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteStaticTranslationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting static translation. TranslationId: {TranslationId}", request.Id);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var translation = await _context.Set<StaticTranslation>()
            .FirstOrDefaultAsync(st => st.Id == request.Id, cancellationToken);

        if (translation == null)
        {
            _logger.LogWarning("Static translation not found for deletion. TranslationId: {TranslationId}", request.Id);
            throw new NotFoundException("Statik çeviri", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
        translation.MarkAsDeleted(); // BaseEntity'den geliyor

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Static translation deleted successfully. TranslationId: {TranslationId}", translation.Id);
        return Unit.Value;
    }
}

