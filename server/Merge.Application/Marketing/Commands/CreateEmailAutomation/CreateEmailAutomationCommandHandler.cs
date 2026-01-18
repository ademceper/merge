using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.CreateEmailAutomation;

public class CreateEmailAutomationCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateEmailAutomationCommandHandler> logger) : IRequestHandler<CreateEmailAutomationCommand, EmailAutomationDto>
{
    public async Task<EmailAutomationDto> Handle(CreateEmailAutomationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Email otomasyonu oluşturuluyor. Name: {Name}, Type: {Type}",
            request.Name, request.Type);

        var typeEnum = Enum.Parse<EmailAutomationType>(request.Type, true);
        var automation = EmailAutomation.Create(
            name: request.Name,
            description: request.Description,
            type: typeEnum,
            templateId: request.TemplateId,
            delayHours: request.DelayHours,
            triggerConditions: request.TriggerConditions != null ? JsonSerializer.Serialize(request.TriggerConditions) : null);

        await context.Set<EmailAutomation>().AddAsync(automation, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var createdAutomation = await context.Set<EmailAutomation>()
            .AsNoTracking()
            .Include(a => a.Template)
            .FirstOrDefaultAsync(a => a.Id == automation.Id, cancellationToken);

        logger.LogInformation(
            "Email otomasyonu oluşturuldu. AutomationId: {AutomationId}, Name: {Name}",
            automation.Id, request.Name);

        return mapper.Map<EmailAutomationDto>(createdAutomation!);
    }
}
