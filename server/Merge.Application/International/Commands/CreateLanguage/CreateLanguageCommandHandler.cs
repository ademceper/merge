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

public class CreateLanguageCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateLanguageCommandHandler> logger) : IRequestHandler<CreateLanguageCommand, LanguageDto>
{
    public async Task<LanguageDto> Handle(CreateLanguageCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating language. Code: {Code}, Name: {Name}", request.Code, request.Name);

        var exists = await context.Set<Language>()
            .AnyAsync(l => l.Code.ToLower() == request.Code.ToLower(), cancellationToken);

        if (exists)
        {
            logger.LogWarning("Language code already exists. Code: {Code}", request.Code);
            throw new BusinessException($"Bu dil kodu zaten mevcut: {request.Code}");
        }

        if (request.IsDefault)
        {
            var currentDefault = await context.Set<Language>()
                .FirstOrDefaultAsync(l => l.IsDefault, cancellationToken);

            if (currentDefault != null)
            {
                currentDefault.RemoveDefaultStatus();
            }
        }

        var language = Language.Create(
            request.Code,
            request.Name,
            request.NativeName,
            request.IsDefault,
            request.IsActive,
            request.IsRTL,
            request.FlagIcon);

        await context.Set<Language>().AddAsync(language, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Language created successfully. LanguageId: {LanguageId}, Code: {Code}", language.Id, language.Code);

        return mapper.Map<LanguageDto>(language);
    }
}
