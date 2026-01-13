using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationEntity = Merge.Domain.Modules.Notifications.Notification;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Notification;
using Merge.Application.Exceptions;
using Merge.Application.Notification.Commands.CreateNotification;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;


namespace Merge.Application.Services.Notification;

public class NotificationTemplateService : INotificationTemplateService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<NotificationTemplateService> _logger;

    public NotificationTemplateService(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IMapper mapper,
        ILogger<NotificationTemplateService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<NotificationTemplateDto> CreateTemplateAsync(CreateNotificationTemplateDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Notification template oluşturuluyor. Name: {Name}, Type: {Type}",
            dto.Name, dto.Type);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var template = NotificationTemplate.Create(
            dto.Name,
            dto.Type,
            dto.TitleTemplate,
            dto.MessageTemplate,
            dto.Description,
            dto.LinkTemplate,
            dto.IsActive,
            dto.Variables != null ? JsonSerializer.Serialize(dto.Variables) : null,
            dto.DefaultData != null ? JsonSerializer.Serialize(dto.DefaultData) : null);

        await _context.Set<NotificationTemplate>().AddAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Notification template oluşturuldu. TemplateId: {TemplateId}, Name: {Name}",
            template.Id, dto.Name);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<NotificationTemplateDto>(template);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<NotificationTemplateDto?> GetTemplateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<NotificationTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return template != null ? _mapper.Map<NotificationTemplateDto>(template) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<NotificationTemplateDto?> GetTemplateByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 1.2: Enum kullanımı (string Type YASAK)
        if (!Enum.TryParse<Merge.Domain.Enums.NotificationType>(type, true, out var notificationTypeEnum))
        {
            return null;
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<NotificationTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Type == notificationTypeEnum && t.IsActive, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return template != null ? _mapper.Map<NotificationTemplateDto>(template) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<NotificationTemplateDto>> GetTemplatesAsync(string? type = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        IQueryable<NotificationTemplate> query = _context.Set<NotificationTemplate>()
            .AsNoTracking();

        if (!string.IsNullOrEmpty(type))
        {
            // ✅ BOLUM 1.2: Enum kullanımı (string Type YASAK)
            if (Enum.TryParse<Merge.Domain.Enums.NotificationType>(type, true, out var notificationTypeEnum))
            {
                query = query.Where(t => t.Type == notificationTypeEnum);
            }
        }

        var templates = await query
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper kullan
        return _mapper.Map<IEnumerable<NotificationTemplateDto>>(templates);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<NotificationTemplateDto> UpdateTemplateAsync(Guid id, UpdateNotificationTemplateDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<NotificationTemplate>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (template == null)
        {
            throw new NotFoundException("Şablon", id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        template.Update(
            dto.Name,
            dto.Description,
            dto.Type,
            dto.TitleTemplate,
            dto.MessageTemplate,
            dto.LinkTemplate,
            dto.IsActive,
            dto.Variables != null ? JsonSerializer.Serialize(dto.Variables) : null,
            dto.DefaultData != null ? JsonSerializer.Serialize(dto.DefaultData) : null);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<NotificationTemplateDto>(template);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteTemplateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<NotificationTemplate>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (template == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        template.Delete();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<NotificationDto> CreateNotificationFromTemplateAsync(
        Guid userId, 
        string templateType, 
        Dictionary<string, object>? variables = null,
        CancellationToken cancellationToken = default)
    {
        var template = await GetTemplateByTypeAsync(templateType, cancellationToken);
        if (template == null)
        {
            throw new NotFoundException("Şablon", Guid.Empty);
        }

        // Merge default data with provided variables
        var allVariables = new Dictionary<string, object>();
        if (template.DefaultData != null)
        {
            // Convert NotificationTemplateSettingsDto to Dictionary
            var defaultDataProps = typeof(NotificationTemplateSettingsDto).GetProperties();
            foreach (var prop in defaultDataProps)
            {
                var value = prop.GetValue(template.DefaultData);
                if (value != null)
                {
                    allVariables[prop.Name] = value;
                }
            }
        }
        if (variables != null)
        {
            foreach (var kvp in variables)
            {
                allVariables[kvp.Key] = kvp.Value;
            }
        }

        // Replace variables in templates
        var title = ReplaceVariables(template.TitleTemplate, allVariables);
        var message = ReplaceVariables(template.MessageTemplate, allVariables);
        var link = template.LinkTemplate != null 
            ? ReplaceVariables(template.LinkTemplate, allVariables) 
            : null;

        // ✅ BOLUM 1.2: Enum kullanımı (string Type YASAK)
        if (!Enum.TryParse<Merge.Domain.Enums.NotificationType>(templateType, true, out var notificationTypeEnum))
        {
            throw new NotFoundException("Şablon", Guid.Empty);
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service yerine MediatR kullan
        var createCommand = new CreateNotificationCommand(
            userId,
            notificationTypeEnum,
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

