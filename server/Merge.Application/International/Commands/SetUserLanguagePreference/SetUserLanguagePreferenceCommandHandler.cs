using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.SetUserLanguagePreference;

public class SetUserLanguagePreferenceCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<SetUserLanguagePreferenceCommandHandler> logger) : IRequestHandler<SetUserLanguagePreferenceCommand, Unit>
{
    public async Task<Unit> Handle(SetUserLanguagePreferenceCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Setting user language preference. UserId: {UserId}, LanguageCode: {LanguageCode}", 
            request.UserId, request.LanguageCode);

        var language = await context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Code.ToLower() == request.LanguageCode.ToLower() && l.IsActive, cancellationToken);

        if (language == null)
        {
            logger.LogWarning("Language not found. LanguageCode: {LanguageCode}", request.LanguageCode);
            throw new NotFoundException("Dil", Guid.Empty);
        }

        var preference = await context.Set<UserLanguagePreference>()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (preference == null)
        {
            preference = UserLanguagePreference.Create(
                request.UserId,
                language.Id,
                language.Code);
            await context.Set<UserLanguagePreference>().AddAsync(preference, cancellationToken);
        }
        else
        {
            preference.UpdateLanguage(language.Id, language.Code);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User language preference set successfully. UserId: {UserId}, LanguageCode: {LanguageCode}", 
            request.UserId, request.LanguageCode);
        return Unit.Value;
    }
}
