using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.DeleteEmailTemplate;

public class DeleteEmailTemplateCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteEmailTemplateCommandHandler> logger) : IRequestHandler<DeleteEmailTemplateCommand, bool>
{
    public async Task<bool> Handle(DeleteEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await context.Set<EmailTemplate>()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (template is null)
        {
            return false;
        }

        template.MarkAsDeleted();
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Email template silindi. TemplateId: {TemplateId}",
            request.Id);

        return true;
    }
}
