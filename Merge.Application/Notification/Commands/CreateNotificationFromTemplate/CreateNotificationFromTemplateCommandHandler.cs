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

namespace Merge.Application.Notification.Commands.CreateNotificationFromTemplate;

/// <summary>
/// Create Notification From Template Command Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CreateNotificationFromTemplateCommandHandler : IRequestHandler<CreateNotificationFromTemplateCommand, NotificationDto>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly ILogger<CreateNotificationFromTemplateCommandHandler> _logger;

    public CreateNotificationFromTemplateCommandHandler(
        IDbContext context,
        IMapper mapper,
        IMediator mediator,
        ILogger<CreateNotificationFromTemplateCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<NotificationDto> Handle(CreateNotificationFromTemplateCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<NotificationTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Type == request.TemplateType && t.IsActive, cancellationToken);

        if (template == null)
        {
            throw new NotFoundException("Şablon", Guid.Empty);
        }

        // Merge default data with provided variables
        var allVariables = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(template.DefaultData))
        {
            // ✅ FIX: template.DefaultData string olarak tutuluyor, deserialize etmemiz gerekiyor
            var defaultData = JsonSerializer.Deserialize<NotificationTemplateSettingsDto>(template.DefaultData);
            if (defaultData != null)
            {
                // ✅ FIX: Record'lar için reflection yerine direkt property'leri kullan
                if (defaultData.DefaultLanguage != null) allVariables[nameof(defaultData.DefaultLanguage)] = defaultData.DefaultLanguage;
                if (defaultData.DefaultSubject != null) allVariables[nameof(defaultData.DefaultSubject)] = defaultData.DefaultSubject;
                if (defaultData.SenderName != null) allVariables[nameof(defaultData.SenderName)] = defaultData.SenderName;
                if (defaultData.SenderEmail != null) allVariables[nameof(defaultData.SenderEmail)] = defaultData.SenderEmail;
                if (defaultData.ReplyToEmail != null) allVariables[nameof(defaultData.ReplyToEmail)] = defaultData.ReplyToEmail;
                allVariables[nameof(defaultData.UseHtmlFormat)] = defaultData.UseHtmlFormat;
                allVariables[nameof(defaultData.TrackingEnabled)] = defaultData.TrackingEnabled;
                allVariables[nameof(defaultData.MaxRetries)] = defaultData.MaxRetries;
                allVariables[nameof(defaultData.RetryIntervalMinutes)] = defaultData.RetryIntervalMinutes;
            }
        }
        if (request.Variables != null)
        {
            // ✅ FIX: Record'lar için reflection yerine direkt property'leri kullan
            if (request.Variables.CustomerName != null) allVariables[nameof(request.Variables.CustomerName)] = request.Variables.CustomerName;
            if (request.Variables.CustomerEmail != null) allVariables[nameof(request.Variables.CustomerEmail)] = request.Variables.CustomerEmail;
            if (request.Variables.OrderNumber != null) allVariables[nameof(request.Variables.OrderNumber)] = request.Variables.OrderNumber;
            if (request.Variables.ActionUrl != null) allVariables[nameof(request.Variables.ActionUrl)] = request.Variables.ActionUrl;
            if (request.Variables.CompanyName != null) allVariables[nameof(request.Variables.CompanyName)] = request.Variables.CompanyName;
            if (request.Variables.LogoUrl != null) allVariables[nameof(request.Variables.LogoUrl)] = request.Variables.LogoUrl;
            if (request.Variables.Amount.HasValue) allVariables[nameof(request.Variables.Amount)] = request.Variables.Amount.Value;
            if (request.Variables.Currency != null) allVariables[nameof(request.Variables.Currency)] = request.Variables.Currency;
            if (request.Variables.ExpirationDate.HasValue) allVariables[nameof(request.Variables.ExpirationDate)] = request.Variables.ExpirationDate.Value;
            if (request.Variables.ProductName != null) allVariables[nameof(request.Variables.ProductName)] = request.Variables.ProductName;
            if (request.Variables.CustomMessage != null) allVariables[nameof(request.Variables.CustomMessage)] = request.Variables.CustomMessage;
        }

        // Replace variables in templates
        var title = ReplaceVariables(template.TitleTemplate, allVariables);
        var message = ReplaceVariables(template.MessageTemplate, allVariables);
        var link = template.LinkTemplate != null 
            ? ReplaceVariables(template.LinkTemplate, allVariables) 
            : null;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - CreateNotificationCommand kullan
        var createCommand = new CreateNotificationCommand(
            request.UserId,
            request.TemplateType,
            title,
            message,
            link);

        return await _mediator.Send(createCommand, cancellationToken);
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
