using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.International.Commands.CreateStaticTranslation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class CreateStaticTranslationCommandHandler : IRequestHandler<CreateStaticTranslationCommand, StaticTranslationDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateStaticTranslationCommandHandler> _logger;

    public CreateStaticTranslationCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateStaticTranslationCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<StaticTranslationDto> Handle(CreateStaticTranslationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating static translation. Key: {Key}, LanguageCode: {LanguageCode}", 
            request.Key, request.LanguageCode);

        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Code.ToLower() == request.LanguageCode.ToLower(), cancellationToken);

        if (language == null)
        {
            _logger.LogWarning("Language not found. LanguageCode: {LanguageCode}", request.LanguageCode);
            throw new NotFoundException("Dil", Guid.Empty);
        }

        // ✅ PERFORMANCE: Removed manual !st.IsDeleted (Global Query Filter)
        var exists = await _context.Set<StaticTranslation>()
            .AnyAsync(st => st.Key == request.Key &&
                           st.LanguageCode.ToLower() == request.LanguageCode.ToLower(), cancellationToken);

        if (exists)
        {
            _logger.LogWarning("Static translation already exists. Key: {Key}, LanguageCode: {LanguageCode}", 
                request.Key, request.LanguageCode);
            throw new BusinessException("Bu anahtar ve dil için çeviri zaten mevcut.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var translation = StaticTranslation.Create(
            request.Key,
            language.Id,
            language.Code,
            request.Value,
            request.Category);

        await _context.Set<StaticTranslation>().AddAsync(translation, cancellationToken);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Static translation created successfully. TranslationId: {TranslationId}", translation.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<StaticTranslationDto>(translation);
    }
}

