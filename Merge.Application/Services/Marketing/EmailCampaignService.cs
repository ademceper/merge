using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Services.Notification;
using Merge.Application.Interfaces.User;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using System.Text.Json;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;


namespace Merge.Application.Services.Marketing;

public class EmailCampaignService : IEmailCampaignService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly ILogger<EmailCampaignService> _logger;

    public EmailCampaignService(ApplicationDbContext context, IUnitOfWork unitOfWork, IEmailService emailService, IMapper mapper, ILogger<EmailCampaignService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _mapper = mapper;
        _logger = logger;
    }

    // Campaign Management
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<EmailCampaignDto> CreateCampaignAsync(CreateEmailCampaignDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email kampanyası oluşturuluyor. Name: {Name}, Type: {Type}, TargetSegment: {TargetSegment}",
            dto.Name, dto.Type, dto.TargetSegment);

        var campaign = new EmailCampaign
        {
            Name = dto.Name,
            Subject = dto.Subject,
            FromName = dto.FromName,
            FromEmail = dto.FromEmail,
            ReplyToEmail = dto.ReplyToEmail,
            TemplateId = dto.TemplateId,
            Content = dto.Content,
            Type = Enum.Parse<EmailCampaignType>(dto.Type, true),
            Status = EmailCampaignStatus.Draft,
            ScheduledAt = dto.ScheduledAt,
            TargetSegment = dto.TargetSegment,
            Tags = dto.Tags != null ? JsonSerializer.Serialize(dto.Tags) : null
        };

        await _context.Set<EmailCampaign>().AddAsync(campaign, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with includes in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var createdCampaign = await _context.Set<EmailCampaign>()
            .AsNoTracking()
            .Include(c => c.Template)
            .FirstOrDefaultAsync(c => c.Id == campaign.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email kampanyası oluşturuldu. CampaignId: {CampaignId}, Name: {Name}",
            campaign.Id, dto.Name);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<EmailCampaignDto>(createdCampaign!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<EmailCampaignDto?> GetCampaignAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
            .AsNoTracking()
            .Include(c => c.Template)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return campaign != null ? _mapper.Map<EmailCampaignDto>(campaign) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
    public async Task<PagedResult<EmailCampaignDto>> GetCampaignsAsync(string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        IQueryable<EmailCampaign> query = _context.Set<EmailCampaign>()
            .AsNoTracking();

        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<EmailCampaignStatus>(status, true, out var statusEnum))
            {
                query = query.Where(c => c.Status == statusEnum);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var campaigns = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return new PagedResult<EmailCampaignDto>
        {
            Items = _mapper.Map<List<EmailCampaignDto>>(campaigns),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<EmailCampaignDto> UpdateCampaignAsync(Guid id, UpdateEmailCampaignDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (campaign == null)
        {
            throw new NotFoundException("Kampanya", id);
        }

        if (campaign.Status != EmailCampaignStatus.Draft)
        {
            throw new BusinessException("Sadece taslak kampanyalar güncellenebilir.");
        }

        if (dto.Name != null) campaign.Name = dto.Name;
        if (dto.Subject != null) campaign.Subject = dto.Subject;
        if (dto.FromName != null) campaign.FromName = dto.FromName;
        if (dto.FromEmail != null) campaign.FromEmail = dto.FromEmail;
        if (dto.ReplyToEmail != null) campaign.ReplyToEmail = dto.ReplyToEmail;
        if (dto.TemplateId.HasValue) campaign.TemplateId = dto.TemplateId;
        if (dto.Content != null) campaign.Content = dto.Content;
        if (dto.ScheduledAt.HasValue) campaign.ScheduledAt = dto.ScheduledAt;
        if (dto.TargetSegment != null) campaign.TargetSegment = dto.TargetSegment;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with includes in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var updatedCampaign = await _context.Set<EmailCampaign>()
            .AsNoTracking()
            .Include(c => c.Template)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<EmailCampaignDto>(updatedCampaign!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteCampaignAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (campaign == null) return false;

        if (campaign.Status == EmailCampaignStatus.Sending)
        {
            throw new BusinessException("Şu anda gönderilmekte olan bir kampanya silinemez.");
        }

        campaign.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> ScheduleCampaignAsync(Guid id, DateTime scheduledAt, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (campaign == null) return false;

        if (scheduledAt <= DateTime.UtcNow)
        {
            throw new ValidationException("Zamanlanmış zaman gelecekte olmalıdır.");
        }

        campaign.ScheduledAt = scheduledAt;
        campaign.Status = EmailCampaignStatus.Scheduled;

        await PrepareCampaignRecipientsAsync(campaign, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> SendCampaignAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
            .Include(c => c.Recipients)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (campaign == null) return false;

        if (campaign.Status == EmailCampaignStatus.Sent)
        {
            throw new BusinessException("Kampanya zaten gönderilmiş.");
        }

        campaign.Status = EmailCampaignStatus.Sending;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Prepare recipients if not already done
        if (campaign.Recipients.Count == 0)
        {
            await PrepareCampaignRecipientsAsync(campaign, cancellationToken);
        }

        // Send emails (in production, this would be queued)
        await SendCampaignEmailsAsync(campaign, cancellationToken);

        campaign.Status = EmailCampaignStatus.Sent;
        campaign.SentAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> PauseCampaignAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (campaign == null) return false;

        if (campaign.Status != EmailCampaignStatus.Sending)
        {
            throw new BusinessException("Sadece gönderilmekte olan kampanyalar duraklatılabilir.");
        }

        campaign.Status = EmailCampaignStatus.Paused;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> CancelCampaignAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (campaign == null) return false;

        campaign.Status = EmailCampaignStatus.Cancelled;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task SendTestEmailAsync(SendTestEmailDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
            .AsNoTracking()
            .Include(c => c.Template)
            .FirstOrDefaultAsync(c => c.Id == dto.CampaignId, cancellationToken);

        if (campaign == null)
        {
            throw new NotFoundException("Kampanya", dto.CampaignId);
        }

        var content = !string.IsNullOrEmpty(campaign.Content)
            ? campaign.Content
            : campaign.Template?.HtmlContent ?? string.Empty;

        await _emailService.SendEmailAsync(
            dto.TestEmail,
            campaign.Subject + " [TEST]",
            content,
            true,
            cancellationToken
        );
    }

    // Template Management
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<EmailTemplateDto> CreateTemplateAsync(CreateEmailTemplateDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email template oluşturuluyor. Name: {Name}, Type: {Type}",
            dto.Name, dto.Type);

        var template = new EmailTemplate
        {
            Name = dto.Name,
            Description = dto.Description,
            Subject = dto.Subject,
            HtmlContent = dto.HtmlContent,
            TextContent = dto.TextContent,
            Type = Enum.Parse<EmailTemplateType>(dto.Type, true),
            Variables = dto.Variables != null ? JsonSerializer.Serialize(dto.Variables) : null
        };

        await _context.Set<EmailTemplate>().AddAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var createdTemplate = await _context.Set<EmailTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == template.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email template oluşturuldu. TemplateId: {TemplateId}, Name: {Name}",
            template.Id, dto.Name);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<EmailTemplateDto>(createdTemplate!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<EmailTemplateDto?> GetTemplateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<EmailTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return template != null ? _mapper.Map<EmailTemplateDto>(template) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<EmailTemplateDto>> GetTemplatesAsync(string? type = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        IQueryable<EmailTemplate> query = _context.Set<EmailTemplate>()
            .AsNoTracking()
            .Where(t => t.IsActive);

        if (!string.IsNullOrEmpty(type))
        {
            if (Enum.TryParse<EmailTemplateType>(type, true, out var typeEnum))
            {
                query = query.Where(t => t.Type == typeEnum);
            }
        }

        var templates = await query
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<EmailTemplateDto>>(templates);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<EmailTemplateDto>> GetTemplatesAsync(string? type, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        IQueryable<EmailTemplate> query = _context.Set<EmailTemplate>()
            .AsNoTracking()
            .Where(t => t.IsActive);

        if (!string.IsNullOrEmpty(type))
        {
            if (Enum.TryParse<EmailTemplateType>(type, true, out var typeEnum))
            {
                query = query.Where(t => t.Type == typeEnum);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var templates = await query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<EmailTemplateDto>
        {
            Items = _mapper.Map<List<EmailTemplateDto>>(templates),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<EmailTemplateDto> UpdateTemplateAsync(Guid id, CreateEmailTemplateDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<EmailTemplate>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (template == null)
        {
            throw new NotFoundException("Şablon", id);
        }

        template.Name = dto.Name;
        template.Description = dto.Description;
        template.Subject = dto.Subject;
        template.HtmlContent = dto.HtmlContent;
        template.TextContent = dto.TextContent;
        template.Type = Enum.Parse<EmailTemplateType>(dto.Type, true);
        template.Variables = dto.Variables != null ? JsonSerializer.Serialize(dto.Variables) : null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var updatedTemplate = await _context.Set<EmailTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<EmailTemplateDto>(updatedTemplate!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteTemplateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<EmailTemplate>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (template == null) return false;

        template.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // Subscriber Management
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<EmailSubscriberDto> SubscribeAsync(CreateEmailSubscriberDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email aboneliği oluşturuluyor. Email: {Email}, Source: {Source}",
            dto.Email, dto.Source);

        var existing = await _context.Set<EmailSubscriber>()
            .FirstOrDefaultAsync(s => s.Email.ToLower() == dto.Email.ToLower(), cancellationToken);

        if (existing != null)
        {
            if (existing.IsDeleted)
            {
                existing.IsDeleted = false;
            }

            existing.IsSubscribed = true;
            existing.SubscribedAt = DateTime.UtcNow;
            existing.UnsubscribedAt = null;
            existing.FirstName = dto.FirstName;
            existing.LastName = dto.LastName;
            existing.Source = dto.Source;
            existing.Tags = dto.Tags != null ? JsonSerializer.Serialize(dto.Tags) : null;
            existing.CustomFields = dto.CustomFields != null ? JsonSerializer.Serialize(dto.CustomFields) : null;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // ✅ PERFORMANCE: Reload in one query (N+1 fix)
            // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
            var reloadedExisting = await _context.Set<EmailSubscriber>()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == existing.Id, cancellationToken);

            // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
            _logger.LogInformation(
                "Email aboneliği güncellendi (mevcut kullanıcı). SubscriberId: {SubscriberId}, Email: {Email}",
                existing.Id, dto.Email);

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return _mapper.Map<EmailSubscriberDto>(reloadedExisting!);
        }

        var subscriber = new EmailSubscriber
        {
            Email = dto.Email.ToLower(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Source = dto.Source,
            Tags = dto.Tags != null ? JsonSerializer.Serialize(dto.Tags) : null,
            CustomFields = dto.CustomFields != null ? JsonSerializer.Serialize(dto.CustomFields) : null
        };

        await _context.Set<EmailSubscriber>().AddAsync(subscriber, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var createdSubscriber = await _context.Set<EmailSubscriber>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == subscriber.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email aboneliği oluşturuldu. SubscriberId: {SubscriberId}, Email: {Email}",
            subscriber.Id, dto.Email);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<EmailSubscriberDto>(createdSubscriber!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> UnsubscribeAsync(string email, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var subscriber = await _context.Set<EmailSubscriber>()
            .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower(), cancellationToken);

        if (subscriber == null) return false;

        subscriber.IsSubscribed = false;
        subscriber.UnsubscribedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<EmailSubscriberDto?> GetSubscriberAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var subscriber = await _context.Set<EmailSubscriber>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return subscriber != null ? _mapper.Map<EmailSubscriberDto>(subscriber) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<EmailSubscriberDto?> GetSubscriberByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var subscriber = await _context.Set<EmailSubscriber>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower(), cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return subscriber != null ? _mapper.Map<EmailSubscriberDto>(subscriber) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
    public async Task<PagedResult<EmailSubscriberDto>> GetSubscribersAsync(bool? isSubscribed = null, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        IQueryable<EmailSubscriber> query = _context.Set<EmailSubscriber>()
            .AsNoTracking();

        if (isSubscribed.HasValue)
        {
            query = query.Where(s => s.IsSubscribed == isSubscribed.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var subscribers = await query
            .OrderByDescending(s => s.SubscribedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return new PagedResult<EmailSubscriberDto>
        {
            Items = _mapper.Map<List<EmailSubscriberDto>>(subscribers),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> UpdateSubscriberAsync(Guid id, CreateEmailSubscriberDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var subscriber = await _context.Set<EmailSubscriber>()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (subscriber == null) return false;

        subscriber.FirstName = dto.FirstName;
        subscriber.LastName = dto.LastName;
        subscriber.Tags = dto.Tags != null ? JsonSerializer.Serialize(dto.Tags) : null;
        subscriber.CustomFields = dto.CustomFields != null ? JsonSerializer.Serialize(dto.CustomFields) : null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<int> BulkImportSubscribersAsync(BulkImportSubscribersDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Subscribers == null || !dto.Subscribers.Any())
        {
            return 0;
        }

        // ✅ PERFORMANCE: Batch load existing subscribers (N+1 fix)
        var emails = dto.Subscribers.Select(s => s.Email.ToLower()).Distinct().ToList();
        var existingSubscribers = await _context.Set<EmailSubscriber>()
            .Where(s => emails.Contains(s.Email.ToLower()))
            .ToDictionaryAsync(s => s.Email.ToLower(), cancellationToken);

        var newSubscribers = new List<EmailSubscriber>();
        var updatedSubscribers = new List<EmailSubscriber>();

        foreach (var subscriberDto in dto.Subscribers)
        {
            var email = subscriberDto.Email.ToLower();
            
            if (existingSubscribers.TryGetValue(email, out var existing))
            {
                // Update existing
                if (existing.IsDeleted)
                {
                    existing.IsDeleted = false;
                }

                existing.IsSubscribed = true;
                existing.SubscribedAt = DateTime.UtcNow;
                existing.UnsubscribedAt = null;
                existing.FirstName = subscriberDto.FirstName;
                existing.LastName = subscriberDto.LastName;
                existing.Source = subscriberDto.Source;
                existing.Tags = subscriberDto.Tags != null ? JsonSerializer.Serialize(subscriberDto.Tags) : null;
                existing.CustomFields = subscriberDto.CustomFields != null ? JsonSerializer.Serialize(subscriberDto.CustomFields) : null;
                
                updatedSubscribers.Add(existing);
            }
            else
            {
                // Create new
                var subscriber = new EmailSubscriber
                {
                    Email = email,
                    FirstName = subscriberDto.FirstName,
                    LastName = subscriberDto.LastName,
                    Source = subscriberDto.Source,
                    Tags = subscriberDto.Tags != null ? JsonSerializer.Serialize(subscriberDto.Tags) : null,
                    CustomFields = subscriberDto.CustomFields != null ? JsonSerializer.Serialize(subscriberDto.CustomFields) : null
                };
                
                newSubscribers.Add(subscriber);
            }
        }

        if (newSubscribers.Count > 0)
        {
            await _context.Set<EmailSubscriber>().AddRangeAsync(newSubscribers, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return newSubscribers.Count + updatedSubscribers.Count;
    }

    // Analytics
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<EmailCampaignAnalyticsDto?> GetCampaignAnalyticsAsync(Guid campaignId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == campaignId, cancellationToken);

        if (campaign == null) return null;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = _mapper.Map<EmailCampaignAnalyticsDto>(campaign);
        
        // ✅ PERFORMANCE: Memory'de minimal işlem (sadece property assignment)
        dto.BounceRate = campaign.SentCount > 0 ? (decimal)campaign.BouncedCount / campaign.SentCount * 100 : 0;
        dto.UnsubscribeRate = campaign.SentCount > 0 ? (decimal)campaign.UnsubscribedCount / campaign.SentCount * 100 : 0;
        
        return dto;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<EmailCampaignStatsDto> GetCampaignStatsAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var totalCampaigns = await _context.Set<EmailCampaign>()
            .CountAsync(cancellationToken);

        var activeCampaigns = await _context.Set<EmailCampaign>()
            .CountAsync(c => c.Status == EmailCampaignStatus.Sending || c.Status == EmailCampaignStatus.Scheduled, cancellationToken);

        var totalSubscribers = await _context.Set<EmailSubscriber>()
            .CountAsync(cancellationToken);

        var activeSubscribers = await _context.Set<EmailSubscriber>()
            .CountAsync(s => s.IsSubscribed, cancellationToken);

        var totalEmailsSent = await _context.Set<EmailCampaign>()
            .SumAsync(c => (long)c.SentCount, cancellationToken);

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var sentCampaignsCount = await _context.Set<EmailCampaign>()
            .CountAsync(c => c.Status == EmailCampaignStatus.Sent, cancellationToken);

        var avgOpenRate = sentCampaignsCount > 0
            ? await _context.Set<EmailCampaign>()
                .Where(c => c.Status == EmailCampaignStatus.Sent)
                .AverageAsync(c => (decimal?)c.OpenRate, cancellationToken) ?? 0
            : 0;

        var avgClickRate = sentCampaignsCount > 0
            ? await _context.Set<EmailCampaign>()
                .Where(c => c.Status == EmailCampaignStatus.Sent)
                .AverageAsync(c => (decimal?)c.ClickRate, cancellationToken) ?? 0
            : 0;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var recentCampaigns = await _context.Set<EmailCampaign>()
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return new EmailCampaignStatsDto
        {
            TotalCampaigns = totalCampaigns,
            ActiveCampaigns = activeCampaigns,
            TotalSubscribers = totalSubscribers,
            ActiveSubscribers = activeSubscribers,
            TotalEmailsSent = totalEmailsSent,
            AverageOpenRate = avgOpenRate,
            AverageClickRate = avgClickRate,
            RecentCampaigns = _mapper.Map<List<EmailCampaignDto>>(recentCampaigns)
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task RecordEmailOpenAsync(Guid campaignId, Guid subscriberId, CancellationToken cancellationToken = default)
    {
        var recipient = await _context.Set<EmailCampaignRecipient>()
            .FirstOrDefaultAsync(r => r.CampaignId == campaignId && r.SubscriberId == subscriberId, cancellationToken);

        if (recipient == null) return;

        if (recipient.OpenedAt == null)
        {
            recipient.OpenedAt = DateTime.UtcNow;
            recipient.Status = EmailRecipientStatus.Opened;

            // ✅ PERFORMANCE: Batch load campaign and subscriber (N+1 fix)
            var campaign = await _context.Set<EmailCampaign>()
                .FirstOrDefaultAsync(c => c.Id == campaignId, cancellationToken);

            if (campaign != null)
            {
                campaign.OpenedCount++;
                campaign.OpenRate = campaign.SentCount > 0 ? (decimal)campaign.OpenedCount / campaign.SentCount * 100 : 0;
            }

            var subscriber = await _context.Set<EmailSubscriber>()
                .FirstOrDefaultAsync(s => s.Id == subscriberId, cancellationToken);

            if (subscriber != null)
            {
                subscriber.EmailsOpened++;
                subscriber.LastEmailOpenedAt = DateTime.UtcNow;
            }
        }

        recipient.OpenCount++;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task RecordEmailClickAsync(Guid campaignId, Guid subscriberId, CancellationToken cancellationToken = default)
    {
        var recipient = await _context.Set<EmailCampaignRecipient>()
            .FirstOrDefaultAsync(r => r.CampaignId == campaignId && r.SubscriberId == subscriberId, cancellationToken);

        if (recipient == null) return;

        if (recipient.ClickedAt == null)
        {
            recipient.ClickedAt = DateTime.UtcNow;
            recipient.Status = EmailRecipientStatus.Clicked;

            // ✅ PERFORMANCE: Batch load campaign and subscriber (N+1 fix)
            var campaign = await _context.Set<EmailCampaign>()
                .FirstOrDefaultAsync(c => c.Id == campaignId, cancellationToken);

            if (campaign != null)
            {
                campaign.ClickedCount++;
                campaign.ClickRate = campaign.SentCount > 0 ? (decimal)campaign.ClickedCount / campaign.SentCount * 100 : 0;
            }

            var subscriber = await _context.Set<EmailSubscriber>()
                .FirstOrDefaultAsync(s => s.Id == subscriberId, cancellationToken);

            if (subscriber != null)
            {
                subscriber.EmailsClicked++;
            }
        }

        recipient.ClickCount++;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // Automation
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<EmailAutomationDto> CreateAutomationAsync(CreateEmailAutomationDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email otomasyonu oluşturuluyor. Name: {Name}, Type: {Type}, DelayHours: {DelayHours}",
            dto.Name, dto.Type, dto.DelayHours);

        var automation = new EmailAutomation
        {
            Name = dto.Name,
            Description = dto.Description,
            Type = Enum.Parse<EmailAutomationType>(dto.Type, true),
            TemplateId = dto.TemplateId,
            DelayHours = dto.DelayHours,
            TriggerConditions = dto.TriggerConditions != null ? JsonSerializer.Serialize(dto.TriggerConditions) : null
        };

        await _context.Set<EmailAutomation>().AddAsync(automation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with includes in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var createdAutomation = await _context.Set<EmailAutomation>()
            .AsNoTracking()
            .Include(a => a.Template)
            .FirstOrDefaultAsync(a => a.Id == automation.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email otomasyonu oluşturuldu. AutomationId: {AutomationId}, Name: {Name}",
            automation.Id, dto.Name);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<EmailAutomationDto>(createdAutomation!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<EmailAutomationDto>> GetAutomationsAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var automations = await _context.Set<EmailAutomation>()
            .AsNoTracking()
            .Include(a => a.Template)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<EmailAutomationDto>>(automations);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<EmailAutomationDto>> GetAutomationsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<EmailAutomation>()
            .AsNoTracking()
            .Include(a => a.Template)
            .OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var automations = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<EmailAutomationDto>
        {
            Items = _mapper.Map<List<EmailAutomationDto>>(automations),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> ToggleAutomationAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var automation = await _context.Set<EmailAutomation>()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (automation == null) return false;

        automation.IsActive = isActive;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteAutomationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var automation = await _context.Set<EmailAutomation>()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (automation == null) return false;

        automation.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // Private Helper Methods
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    private async Task PrepareCampaignRecipientsAsync(EmailCampaign campaign, CancellationToken cancellationToken = default)
    {
        var subscribers = await GetTargetedSubscribersAsync(campaign.TargetSegment, cancellationToken);

        foreach (var subscriber in subscribers)
        {
            var recipient = new EmailCampaignRecipient
            {
                CampaignId = campaign.Id,
                SubscriberId = subscriber.Id,
                Status = EmailRecipientStatus.Pending
            };

            await _context.Set<EmailCampaignRecipient>().AddAsync(recipient, cancellationToken);
        }

        campaign.TotalRecipients = subscribers.Count;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    private async Task<List<EmailSubscriber>> GetTargetedSubscribersAsync(string segment, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        IQueryable<EmailSubscriber> query = _context.Set<EmailSubscriber>()
            .Where(s => s.IsSubscribed);

        // Apply segment filters
        switch (segment.ToLower())
        {
            case "all":
                break;
            case "active":
                query = query.Where(s => s.EmailsOpened > 0 || s.EmailsClicked > 0);
                break;
            case "inactive":
                query = query.Where(s => s.EmailsOpened == 0 && s.EmailsClicked == 0);
                break;
            default:
                break;
        }

        return await query.ToListAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    private async Task SendCampaignEmailsAsync(EmailCampaign campaign, CancellationToken cancellationToken = default)
    {
        var recipients = await _context.Set<EmailCampaignRecipient>()
            .Include(r => r.Subscriber)
            .Where(r => r.CampaignId == campaign.Id && r.Status == EmailRecipientStatus.Pending)
            .ToListAsync(cancellationToken);

        var content = !string.IsNullOrEmpty(campaign.Content)
            ? campaign.Content
            : campaign.Template?.HtmlContent ?? string.Empty;

        foreach (var recipient in recipients)
        {
            try
            {
                await _emailService.SendEmailAsync(
                    recipient.Subscriber.Email,
                    campaign.Subject,
                    content,
                    true,
                    cancellationToken
                );

                recipient.Status = EmailRecipientStatus.Sent;
                recipient.SentAt = DateTime.UtcNow;
                campaign.SentCount++;

                var subscriber = recipient.Subscriber;
                subscriber.EmailsSent++;
                subscriber.LastEmailSentAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                // ✅ BOLUM 2.1: Exception handling - Exception yutulmuyor, loglanıyor ve işlem devam ediyor
                _logger.LogError(ex, "Email gönderilemedi. CampaignId: {CampaignId}, SubscriberId: {SubscriberId}", 
                    campaign.Id, recipient.SubscriberId);
                recipient.Status = EmailRecipientStatus.Failed;
                recipient.ErrorMessage = ex.Message;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

}

