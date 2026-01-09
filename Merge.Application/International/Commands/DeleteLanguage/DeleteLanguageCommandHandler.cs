using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.International.Commands.DeleteLanguage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class DeleteLanguageCommandHandler : IRequestHandler<DeleteLanguageCommand, Unit>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteLanguageCommandHandler> _logger;

    public DeleteLanguageCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteLanguageCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteLanguageCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting language. LanguageId: {LanguageId}", request.Id);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var language = await _context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (language == null)
        {
            _logger.LogWarning("Language not found. LanguageId: {LanguageId}", request.Id);
            throw new NotFoundException("Dil", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        language.MarkAsDeleted();

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Language deleted successfully. LanguageId: {LanguageId}, Code: {Code}", language.Id, language.Code);
        return Unit.Value;
    }
}

