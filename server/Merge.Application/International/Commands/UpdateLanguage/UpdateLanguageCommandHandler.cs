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

namespace Merge.Application.International.Commands.UpdateLanguage;

public class UpdateLanguageCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateLanguageCommandHandler> logger) : IRequestHandler<UpdateLanguageCommand, LanguageDto>
{
    public async Task<LanguageDto> Handle(UpdateLanguageCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating language. LanguageId: {LanguageId}", request.Id);

        var language = await context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (language is null)
        {
            logger.LogWarning("Language not found. LanguageId: {LanguageId}", request.Id);
            throw new NotFoundException("Dil", request.Id);
        }

        language.UpdateDetails(request.Name, request.NativeName, request.IsRTL, request.FlagIcon);

        if (request.IsActive && !language.IsActive)
        {
            language.Activate();
        }
        else if (!request.IsActive && language.IsActive)
        {
            language.Deactivate();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Language updated successfully. LanguageId: {LanguageId}, Code: {Code}", language.Id, language.Code);

        return mapper.Map<LanguageDto>(language);
    }
}
