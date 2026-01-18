using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.DeleteLanguage;

public class DeleteLanguageCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteLanguageCommandHandler> logger) : IRequestHandler<DeleteLanguageCommand, Unit>
{
    public async Task<Unit> Handle(DeleteLanguageCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting language. LanguageId: {LanguageId}", request.Id);

        var language = await context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (language is null)
        {
            logger.LogWarning("Language not found. LanguageId: {LanguageId}", request.Id);
            throw new NotFoundException("Dil", request.Id);
        }

        language.MarkAsDeleted();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Language deleted successfully. LanguageId: {LanguageId}, Code: {Code}", language.Id, language.Code);
        return Unit.Value;
    }
}
