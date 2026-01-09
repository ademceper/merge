using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.International.Commands.CreateCategoryTranslation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class CreateCategoryTranslationCommandHandler : IRequestHandler<CreateCategoryTranslationCommand, CategoryTranslationDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCategoryTranslationCommandHandler> _logger;

    public CreateCategoryTranslationCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateCategoryTranslationCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CategoryTranslationDto> Handle(CreateCategoryTranslationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating category translation. CategoryId: {CategoryId}, LanguageCode: {LanguageCode}", 
            request.CategoryId, request.LanguageCode);

        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Code.ToLower() == request.LanguageCode.ToLower(), cancellationToken);

        if (language == null)
        {
            _logger.LogWarning("Language not found. LanguageCode: {LanguageCode}", request.LanguageCode);
            throw new NotFoundException("Dil", Guid.Empty);
        }

        // ✅ PERFORMANCE: Removed manual !ct.IsDeleted (Global Query Filter)
        var exists = await _context.Set<CategoryTranslation>()
            .AnyAsync(ct => ct.CategoryId == request.CategoryId &&
                           ct.LanguageCode.ToLower() == request.LanguageCode.ToLower(), cancellationToken);

        if (exists)
        {
            _logger.LogWarning("Category translation already exists. CategoryId: {CategoryId}, LanguageCode: {LanguageCode}", 
                request.CategoryId, request.LanguageCode);
            throw new BusinessException("Bu kategori ve dil için çeviri zaten mevcut.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var translation = CategoryTranslation.Create(
            request.CategoryId,
            language.Id,
            language.Code,
            request.Name,
            request.Description);

        await _context.Set<CategoryTranslation>().AddAsync(translation, cancellationToken);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category translation created successfully. TranslationId: {TranslationId}", translation.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<CategoryTranslationDto>(translation);
    }
}

