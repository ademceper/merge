using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationEntity = Merge.Domain.Entities.Notification;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Notification;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using System.Text.Json;
using Merge.Application.DTOs.Notification;


namespace Merge.Application.Services.Notification;

public class NotificationTemplateService : INotificationTemplateService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;
    private readonly ILogger<NotificationTemplateService> _logger;

    public NotificationTemplateService(
        IDbContext context,
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IMapper mapper,
        ILogger<NotificationTemplateService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
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

        var template = new NotificationTemplate
        {
            Name = dto.Name,
            Description = dto.Description,
            Type = dto.Type,
            TitleTemplate = dto.TitleTemplate,
            MessageTemplate = dto.MessageTemplate,
            LinkTemplate = dto.LinkTemplate,
            IsActive = dto.IsActive,
            Variables = dto.Variables != null ? JsonSerializer.Serialize(dto.Variables) : null,
            DefaultData = dto.DefaultData != null ? JsonSerializer.Serialize(dto.DefaultData) : null
        };

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
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<NotificationTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Type == type && t.IsActive, cancellationToken);

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
            query = query.Where(t => t.Type == type);
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

        if (!string.IsNullOrEmpty(dto.Name))
            template.Name = dto.Name;
        if (!string.IsNullOrEmpty(dto.Description))
            template.Description = dto.Description;
        if (!string.IsNullOrEmpty(dto.Type))
            template.Type = dto.Type;
        if (!string.IsNullOrEmpty(dto.TitleTemplate))
            template.TitleTemplate = dto.TitleTemplate;
        if (!string.IsNullOrEmpty(dto.MessageTemplate))
            template.MessageTemplate = dto.MessageTemplate;
        if (dto.LinkTemplate != null)
            template.LinkTemplate = dto.LinkTemplate;
        if (dto.IsActive.HasValue)
            template.IsActive = dto.IsActive.Value;
        if (dto.Variables != null)
            template.Variables = JsonSerializer.Serialize(dto.Variables);
        if (dto.DefaultData != null)
            template.DefaultData = JsonSerializer.Serialize(dto.DefaultData);

        template.UpdatedAt = DateTime.UtcNow;
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

        template.IsDeleted = true;
        template.UpdatedAt = DateTime.UtcNow;
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

        var createDto = new CreateNotificationDto
        {
            UserId = userId,
            Type = templateType,
            Title = title,
            Message = message,
            Link = link
        };

        return await _notificationService.CreateNotificationAsync(createDto, cancellationToken);
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

