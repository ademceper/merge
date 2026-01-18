using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.Interfaces;
using Merge.Application.DTOs.Notification;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Commands.UpdateTemplate;


public class UpdateTemplateCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateTemplateCommandHandler> logger) : IRequestHandler<UpdateTemplateCommand, NotificationTemplateDto>
{

    public async Task<NotificationTemplateDto> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await context.Set<NotificationTemplate>()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (template == null)
        {
            throw new NotFoundException("Şablon", request.Id);
        }

        template.Update(
            request.Dto.Name,
            request.Dto.Description,
            request.Dto.Type,
            request.Dto.TitleTemplate,
            request.Dto.MessageTemplate,
            request.Dto.LinkTemplate,
            request.Dto.IsActive,
            request.Dto.Variables != null ? JsonSerializer.Serialize(request.Dto.Variables) : null,
            request.Dto.DefaultData != null ? JsonSerializer.Serialize(request.Dto.DefaultData) : null);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Notification template güncellendi. TemplateId: {TemplateId}",
            request.Id);

        return mapper.Map<NotificationTemplateDto>(template);
    }
}
