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
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.CreateEmailTemplate;

public class CreateEmailTemplateCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateEmailTemplateCommandHandler> logger) : IRequestHandler<CreateEmailTemplateCommand, EmailTemplateDto>
{
    public async Task<EmailTemplateDto> Handle(CreateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Email template oluşturuluyor. Name: {Name}, Type: {Type}",
            request.Name, request.Type);

        var typeEnum = Enum.Parse<EmailTemplateType>(request.Type, true);
        var template = EmailTemplate.Create(
            name: request.Name,
            description: request.Description,
            subject: request.Subject,
            htmlContent: request.HtmlContent,
            textContent: request.TextContent,
            type: typeEnum,
            variables: request.Variables != null ? JsonSerializer.Serialize(request.Variables) : null,
            thumbnail: request.Thumbnail);

        await context.Set<EmailTemplate>().AddAsync(template, cancellationToken);
        
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var createdTemplate = await context.Set<EmailTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == template.Id, cancellationToken);

        logger.LogInformation(
            "Email template oluşturuldu. TemplateId: {TemplateId}, Name: {Name}",
            template.Id, request.Name);

        return mapper.Map<EmailTemplateDto>(createdTemplate!);
    }
}
