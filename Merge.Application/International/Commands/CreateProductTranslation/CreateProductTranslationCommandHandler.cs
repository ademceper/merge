using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.CreateProductTranslation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class CreateProductTranslationCommandHandler : IRequestHandler<CreateProductTranslationCommand, ProductTranslationDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateProductTranslationCommandHandler> _logger;

    public CreateProductTranslationCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateProductTranslationCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProductTranslationDto> Handle(CreateProductTranslationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating product translation. ProductId: {ProductId}, LanguageCode: {LanguageCode}", 
            request.ProductId, request.LanguageCode);

        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Code.ToLower() == request.LanguageCode.ToLower(), cancellationToken);

        if (language == null)
        {
            _logger.LogWarning("Language not found. LanguageCode: {LanguageCode}", request.LanguageCode);
            throw new NotFoundException("Dil", Guid.Empty);
        }

        // ✅ PERFORMANCE: Removed manual !pt.IsDeleted (Global Query Filter)
        var exists = await _context.Set<ProductTranslation>()
            .AnyAsync(pt => pt.ProductId == request.ProductId &&
                           pt.LanguageCode.ToLower() == request.LanguageCode.ToLower(), cancellationToken);

        if (exists)
        {
            _logger.LogWarning("Product translation already exists. ProductId: {ProductId}, LanguageCode: {LanguageCode}", 
                request.ProductId, request.LanguageCode);
            throw new BusinessException("Bu ürün ve dil için çeviri zaten mevcut.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var translation = ProductTranslation.Create(
            request.ProductId,
            language.Id,
            language.Code,
            request.Name,
            request.Description,
            request.ShortDescription,
            request.MetaTitle,
            request.MetaDescription,
            request.MetaKeywords);

        await _context.Set<ProductTranslation>().AddAsync(translation, cancellationToken);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product translation created successfully. TranslationId: {TranslationId}", translation.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ProductTranslationDto>(translation);
    }
}

