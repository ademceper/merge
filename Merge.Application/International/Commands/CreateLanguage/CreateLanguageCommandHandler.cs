using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.CreateLanguage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class CreateLanguageCommandHandler : IRequestHandler<CreateLanguageCommand, LanguageDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateLanguageCommandHandler> _logger;

    public CreateLanguageCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateLanguageCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LanguageDto> Handle(CreateLanguageCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating language. Code: {Code}, Name: {Name}", request.Code, request.Name);

        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var exists = await _context.Set<Language>()
            .AnyAsync(l => l.Code.ToLower() == request.Code.ToLower(), cancellationToken);

        if (exists)
        {
            _logger.LogWarning("Language code already exists. Code: {Code}", request.Code);
            throw new BusinessException($"Bu dil kodu zaten mevcut: {request.Code}");
        }

        if (request.IsDefault)
        {
            // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
            var currentDefault = await _context.Set<Language>()
                .FirstOrDefaultAsync(l => l.IsDefault, cancellationToken);

            if (currentDefault != null)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                currentDefault.RemoveDefaultStatus();
            }
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var language = Language.Create(
            request.Code,
            request.Name,
            request.NativeName,
            request.IsDefault,
            request.IsActive,
            request.IsRTL,
            request.FlagIcon);

        await _context.Set<Language>().AddAsync(language, cancellationToken);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Language created successfully. LanguageId: {LanguageId}, Code: {Code}", language.Id, language.Code);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<LanguageDto>(language);
    }
}

