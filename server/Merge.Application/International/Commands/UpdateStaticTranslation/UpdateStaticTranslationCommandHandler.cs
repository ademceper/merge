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

namespace Merge.Application.International.Commands.UpdateStaticTranslation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class UpdateStaticTranslationCommandHandler : IRequestHandler<UpdateStaticTranslationCommand, StaticTranslationDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateStaticTranslationCommandHandler> _logger;

    public UpdateStaticTranslationCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateStaticTranslationCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<StaticTranslationDto> Handle(UpdateStaticTranslationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating static translation. TranslationId: {TranslationId}", request.Id);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var translation = await _context.Set<StaticTranslation>()
            .FirstOrDefaultAsync(st => st.Id == request.Id, cancellationToken);

        if (translation == null)
        {
            _logger.LogWarning("Static translation not found. TranslationId: {TranslationId}", request.Id);
            throw new NotFoundException("Statik çeviri", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        translation.Update(request.Value, request.Category);

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Static translation updated successfully. TranslationId: {TranslationId}", translation.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<StaticTranslationDto>(translation);
    }
}

