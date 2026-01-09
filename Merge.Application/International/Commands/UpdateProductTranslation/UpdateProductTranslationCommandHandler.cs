using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.International.Commands.UpdateProductTranslation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class UpdateProductTranslationCommandHandler : IRequestHandler<UpdateProductTranslationCommand, ProductTranslationDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateProductTranslationCommandHandler> _logger;

    public UpdateProductTranslationCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateProductTranslationCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProductTranslationDto> Handle(UpdateProductTranslationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating product translation. TranslationId: {TranslationId}", request.Id);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var translation = await _context.Set<ProductTranslation>()
            .FirstOrDefaultAsync(pt => pt.Id == request.Id, cancellationToken);

        if (translation == null)
        {
            _logger.LogWarning("Product translation not found. TranslationId: {TranslationId}", request.Id);
            throw new NotFoundException("Ürün çevirisi", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        translation.Update(
            request.Name,
            request.Description,
            request.ShortDescription,
            request.MetaTitle,
            request.MetaDescription,
            request.MetaKeywords);

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product translation updated successfully. TranslationId: {TranslationId}", translation.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ProductTranslationDto>(translation);
    }
}

