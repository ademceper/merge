using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.Interfaces;
using Merge.Application.DTOs.Notification;
using Merge.Application.Exceptions;
using Merge.Application.Notification.Commands.CreateNotification;
using Merge.Domain.Entities;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Commands.CreateNotificationFromTemplate;


public class CreateNotificationFromTemplateCommandHandler(IDbContext context, IMapper mapper, IMediator mediator, ILogger<CreateNotificationFromTemplateCommandHandler> logger) : IRequestHandler<CreateNotificationFromTemplateCommand, NotificationDto>
{

    public async Task<NotificationDto> Handle(CreateNotificationFromTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await context.Set<NotificationTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Type == request.TemplateType && t.IsActive, cancellationToken);

        if (template is null)
        {
            throw new NotFoundException("Şablon", Guid.Empty);
        }

        // Merge default data with provided variables
        Dictionary<string, object> allVariables = [];
        if (!string.IsNullOrEmpty(template.DefaultData))
        {
            var defaultData = JsonSerializer.Deserialize<NotificationTemplateSettingsDto>(template.DefaultData);
            if (defaultData is not null)
            {
                if (defaultData.DefaultLanguage is not null) allVariables[nameof(defaultData.DefaultLanguage)] = defaultData.DefaultLanguage;
                if (defaultData.DefaultSubject is not null) allVariables[nameof(defaultData.DefaultSubject)] = defaultData.DefaultSubject;
                if (defaultData.SenderName is not null) allVariables[nameof(defaultData.SenderName)] = defaultData.SenderName;
                if (defaultData.SenderEmail is not null) allVariables[nameof(defaultData.SenderEmail)] = defaultData.SenderEmail;
                if (defaultData.ReplyToEmail is not null) allVariables[nameof(defaultData.ReplyToEmail)] = defaultData.ReplyToEmail;
                allVariables[nameof(defaultData.UseHtmlFormat)] = defaultData.UseHtmlFormat;
                allVariables[nameof(defaultData.TrackingEnabled)] = defaultData.TrackingEnabled;
                allVariables[nameof(defaultData.MaxRetries)] = defaultData.MaxRetries;
                allVariables[nameof(defaultData.RetryIntervalMinutes)] = defaultData.RetryIntervalMinutes;
            }
        }
        if (request.Variables is not null)
        {
            if (request.Variables.CustomerName is not null) allVariables[nameof(request.Variables.CustomerName)] = request.Variables.CustomerName;
            if (request.Variables.CustomerEmail is not null) allVariables[nameof(request.Variables.CustomerEmail)] = request.Variables.CustomerEmail;
            if (request.Variables.OrderNumber is not null) allVariables[nameof(request.Variables.OrderNumber)] = request.Variables.OrderNumber;
            if (request.Variables.ActionUrl is not null) allVariables[nameof(request.Variables.ActionUrl)] = request.Variables.ActionUrl;
            if (request.Variables.CompanyName is not null) allVariables[nameof(request.Variables.CompanyName)] = request.Variables.CompanyName;
            if (request.Variables.LogoUrl is not null) allVariables[nameof(request.Variables.LogoUrl)] = request.Variables.LogoUrl;
            if (request.Variables.Amount.HasValue) allVariables[nameof(request.Variables.Amount)] = request.Variables.Amount.Value;
            if (request.Variables.Currency is not null) allVariables[nameof(request.Variables.Currency)] = request.Variables.Currency;
            if (request.Variables.ExpirationDate.HasValue) allVariables[nameof(request.Variables.ExpirationDate)] = request.Variables.ExpirationDate.Value;
            if (request.Variables.ProductName is not null) allVariables[nameof(request.Variables.ProductName)] = request.Variables.ProductName;
            if (request.Variables.CustomMessage is not null) allVariables[nameof(request.Variables.CustomMessage)] = request.Variables.CustomMessage;
        }

        // Replace variables in templates
        var title = ReplaceVariables(template.TitleTemplate, allVariables);
        var message = ReplaceVariables(template.MessageTemplate, allVariables);
        var link = template.LinkTemplate is not null 
            ? ReplaceVariables(template.LinkTemplate, allVariables) 
            : null;

        var createCommand = new CreateNotificationCommand(
            request.UserId,
            request.TemplateType,
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
