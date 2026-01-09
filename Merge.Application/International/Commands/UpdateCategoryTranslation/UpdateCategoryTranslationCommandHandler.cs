using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.International.Commands.UpdateCategoryTranslation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class UpdateCategoryTranslationCommandHandler : IRequestHandler<UpdateCategoryTranslationCommand, CategoryTranslationDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateCategoryTranslationCommandHandler> _logger;

    public UpdateCategoryTranslationCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateCategoryTranslationCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CategoryTranslationDto> Handle(UpdateCategoryTranslationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating category translation. TranslationId: {TranslationId}", request.Id);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var translation = await _context.Set<CategoryTranslation>()
            .FirstOrDefaultAsync(ct => ct.Id == request.Id, cancellationToken);

        if (translation == null)
        {
            _logger.LogWarning("Category translation not found. TranslationId: {TranslationId}", request.Id);
            throw new NotFoundException("Kategori çevirisi", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        translation.Update(request.Name, request.Description);

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category translation updated successfully. TranslationId: {TranslationId}", translation.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<CategoryTranslationDto>(translation);
    }
}

