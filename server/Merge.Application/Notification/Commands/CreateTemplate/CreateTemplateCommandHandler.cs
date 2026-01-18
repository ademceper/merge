using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.Interfaces;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Entities;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Commands.CreateTemplate;


public class CreateTemplateCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateTemplateCommandHandler> logger) : IRequestHandler<CreateTemplateCommand, NotificationTemplateDto>
{

    public async Task<NotificationTemplateDto> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Notification template oluşturuluyor. Name: {Name}, Type: {Type}",
            request.Dto.Name, request.Dto.Type);

        var template = NotificationTemplate.Create(
            request.Dto.Name,
            request.Dto.Type,
            request.Dto.TitleTemplate,
            request.Dto.MessageTemplate,
            request.Dto.Description,
            request.Dto.LinkTemplate,
            request.Dto.IsActive,
            request.Dto.Variables != null ? JsonSerializer.Serialize(request.Dto.Variables) : null,
            request.Dto.DefaultData != null ? JsonSerializer.Serialize(request.Dto.DefaultData) : null);

        await context.Set<NotificationTemplate>().AddAsync(template, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Notification template oluşturuldu. TemplateId: {TemplateId}, Name: {Name}",
            template.Id, request.Dto.Name);

        return mapper.Map<NotificationTemplateDto>(template);
    }
}
