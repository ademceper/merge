using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.International.Commands.UpdateLanguage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class UpdateLanguageCommandHandler : IRequestHandler<UpdateLanguageCommand, LanguageDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateLanguageCommandHandler> _logger;

    public UpdateLanguageCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateLanguageCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LanguageDto> Handle(UpdateLanguageCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating language. LanguageId: {LanguageId}", request.Id);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var language = await _context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (language == null)
        {
            _logger.LogWarning("Language not found. LanguageId: {LanguageId}", request.Id);
            throw new NotFoundException("Dil", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        language.UpdateDetails(request.Name, request.NativeName, request.IsRTL, request.FlagIcon);

        if (request.IsActive && !language.IsActive)
        {
            language.Activate();
        }
        else if (!request.IsActive && language.IsActive)
        {
            language.Deactivate();
        }

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Language updated successfully. LanguageId: {LanguageId}, Code: {Code}", language.Id, language.Code);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<LanguageDto>(language);
    }
}

