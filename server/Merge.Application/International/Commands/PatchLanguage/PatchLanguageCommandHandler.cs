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

namespace Merge.Application.International.Commands.PatchLanguage;

/// <summary>
/// Handler for PatchLanguageCommand
/// HIGH-API-001: PATCH Support - Partial updates implementation
/// </summary>
public class PatchLanguageCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<PatchLanguageCommandHandler> logger) : IRequestHandler<PatchLanguageCommand, LanguageDto>
{
    public async Task<LanguageDto> Handle(PatchLanguageCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching language. LanguageId: {LanguageId}", request.Id);

        var language = await context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (language is null)
        {
            logger.LogWarning("Language not found. LanguageId: {LanguageId}", request.Id);
            throw new NotFoundException("Dil", request.Id);
        }

        // Apply partial updates - get existing values if not provided
        var name = request.PatchDto.Name ?? language.Name;
        var nativeName = request.PatchDto.NativeName ?? language.NativeName;
        var isRTL = request.PatchDto.IsRTL ?? language.IsRTL;
        var flagIcon = request.PatchDto.FlagIcon ?? language.FlagIcon;

        language.UpdateDetails(name, nativeName, isRTL, flagIcon);

        if (request.PatchDto.IsActive.HasValue)
        {
            if (request.PatchDto.IsActive.Value && !language.IsActive)
            {
                language.Activate();
            }
            else if (!request.PatchDto.IsActive.Value && language.IsActive)
            {
                language.Deactivate();
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Language patched successfully. LanguageId: {LanguageId}, Code: {Code}", language.Id, language.Code);

        return mapper.Map<LanguageDto>(language);
    }
}
