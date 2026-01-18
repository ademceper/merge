using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationEntity = Merge.Domain.Modules.Notifications.Notification;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Notification;
using Merge.Application.Exceptions;
using Merge.Application.Notification.Commands.CreateNotification;
using CreateNotificationCommand = Merge.Application.Notification.Commands.CreateNotification.CreateNotificationCommand;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Services.Notification;

public class NotificationTemplateService(IDbContext context, IUnitOfWork unitOfWork, IMediator mediator, IMapper mapper, ILogger<NotificationTemplateService> logger) : INotificationTemplateService
{

    public async Task<NotificationTemplateDto> CreateTemplateAsync(CreateNotificationTemplateDto dto, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Notification template oluşturuluyor. Name: {Name}, Type: {Type}",
            dto.Name, dto.Type);

        var template = NotificationTemplate.Create(
            dto.Name,
            dto.Type,
            dto.TitleTemplate,
            dto.MessageTemplate,
            dto.Description,
            dto.LinkTemplate,
            dto.IsActive,
            dto.Variables is not null ? JsonSerializer.Serialize(dto.Variables) : null,
            dto.DefaultData is not null ? JsonSerializer.Serialize(dto.DefaultData) : null);

        await context.Set<NotificationTemplate>().AddAsync(template, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Notification template oluşturuldu. TemplateId: {TemplateId}, Name: {Name}",
            template.Id, dto.Name);

        return mapper.Map<NotificationTemplateDto>(template);
    }

    public async Task<NotificationTemplateDto?> GetTemplateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await context.Set<NotificationTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return template is not null ? mapper.Map<NotificationTemplateDto>(template) : null;
    }

    public async Task<NotificationTemplateDto?> GetTemplateByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<Merge.Domain.Enums.NotificationType>(type, true, out var notificationTypeEnum))
        {
            return null;
        }

        var template = await context.Set<NotificationTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Type == notificationTypeEnum && t.IsActive, cancellationToken);

        return template is not null ? mapper.Map<NotificationTemplateDto>(template) : null;
    }

    public async Task<IEnumerable<NotificationTemplateDto>> GetTemplatesAsync(string? type = null, CancellationToken cancellationToken = default)
    {
        IQueryable<NotificationTemplate> query = context.Set<NotificationTemplate>()
            .AsNoTracking();

        if (!string.IsNullOrEmpty(type))
        {
            if (Enum.TryParse<Merge.Domain.Enums.NotificationType>(type, true, out var notificationTypeEnum))
            {
                query = query.Where(t => t.Type == notificationTypeEnum);
            }
        }

        var templates = await query
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<NotificationTemplateDto>>(templates);
    }

    public async Task<NotificationTemplateDto> UpdateTemplateAsync(Guid id, UpdateNotificationTemplateDto dto, CancellationToken cancellationToken = default)
    {
        var template = await context.Set<NotificationTemplate>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (template is null)
        {
            throw new NotFoundException("Şablon", id);
        }

        template.Update(
            dto.Name,
            dto.Description,
            dto.Type,
            dto.TitleTemplate,
            dto.MessageTemplate,
            dto.LinkTemplate,
            dto.IsActive,
            dto.Variables is not null ? JsonSerializer.Serialize(dto.Variables) : null,
            dto.DefaultData is not null ? JsonSerializer.Serialize(dto.DefaultData) : null);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<NotificationTemplateDto>(template);
    }

    public async Task<bool> DeleteTemplateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await context.Set<NotificationTemplate>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (template is null) return false;

        template.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<NotificationDto> CreateNotificationFromTemplateAsync(
        Guid userId, 
        string templateType, 
        Dictionary<string, object>? variables = null,
        CancellationToken cancellationToken = default)
    {
        var template = await GetTemplateByTypeAsync(templateType, cancellationToken);
        if (template is null)
        {
            throw new NotFoundException("Şablon", Guid.Empty);
        }

        // Merge default data with provided variables
        Dictionary<string, object> allVariables = [];
        if (template.DefaultData is not null)
        {
            // Convert NotificationTemplateSettingsDto to Dictionary
            var defaultDataProps = typeof(NotificationTemplateSettingsDto).GetProperties();
            foreach (var prop in defaultDataProps)
            {
                var value = prop.GetValue(template.DefaultData);
                if (value is not null)
                {
                    allVariables[prop.Name] = value;
                }
            }
        }
        if (variables is not null)
        {
            foreach (var kvp in variables)
            {
                allVariables[kvp.Key] = kvp.Value;
            }
        }

        // Replace variables in templates
        var title = ReplaceVariables(template.TitleTemplate, allVariables);
        var message = ReplaceVariables(template.MessageTemplate, allVariables);
        var link = template.LinkTemplate is not null 
            ? ReplaceVariables(template.LinkTemplate, allVariables) 
            : null;

        if (!Enum.TryParse<Merge.Domain.Enums.NotificationType>(templateType, true, out var notificationTypeEnum))
        {
            throw new NotFoundException("Şablon", Guid.Empty);
        }

        var createCommand = new CreateNotificationCommand(
            userId,
            notificationTypeEnum,
            title,
            message,
            link);

        return await mediator.Send(createCommand, cancellationToken);
    }

    private string ReplaceVariables(string template, Dictionary<string, object> variables)
    {
        var result = template;
        foreach (var variable in variables)
        {
            result = result.Replace($"{{{variable.Key}}}", variable.Value?.ToString() ?? string.Empty);
        }
        return result;
    }

}

