using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.UpdateEmailTemplate;

public class UpdateEmailTemplateCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateEmailTemplateCommandHandler> logger) : IRequestHandler<UpdateEmailTemplateCommand, EmailTemplateDto>
{
    public async Task<EmailTemplateDto> Handle(UpdateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await context.Set<EmailTemplate>()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (template == null)
        {
            throw new NotFoundException("Şablon", request.Id);
        }

        EmailTemplateType? typeEnum = null;
        if (!string.IsNullOrEmpty(request.Type))
            typeEnum = Enum.Parse<EmailTemplateType>(request.Type, true);

        template.UpdateDetails(
            name: request.Name,
            description: request.Description,
            subject: request.Subject,
            htmlContent: request.HtmlContent,
            textContent: request.TextContent,
            type: typeEnum,
            variables: request.Variables != null ? JsonSerializer.Serialize(request.Variables) : null,
            thumbnail: request.Thumbnail);

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
                template.Activate();
            else
                template.Deactivate();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedTemplate = await context.Set<EmailTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        return mapper.Map<EmailTemplateDto>(updatedTemplate!);
    }
}
