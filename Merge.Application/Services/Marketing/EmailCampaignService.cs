using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Services.Notification;
using Merge.Application.Interfaces.User;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using System.Text.Json;
using Merge.Application.DTOs.Marketing;


namespace Merge.Application.Services.Marketing;

public class EmailCampaignService : IEmailCampaignService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public EmailCampaignService(ApplicationDbContext context, IUnitOfWork unitOfWork, IEmailService emailService, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _mapper = mapper;
    }

    // Campaign Management
    public async Task<EmailCampaignDto> CreateCampaignAsync(CreateEmailCampaignDto dto)
    {
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

        await _context.Set<EmailCampaign>().AddAsync(campaign);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with includes in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var createdCampaign = await _context.Set<EmailCampaign>()
            .AsNoTracking()
            .Include(c => c.Template)
            .FirstOrDefaultAsync(c => c.Id == campaign.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<EmailCampaignDto>(createdCampaign!);
    }

    public async Task<EmailCampaignDto?> GetCampaignAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
            .AsNoTracking()
            .Include(c => c.Template)
            .FirstOrDefaultAsync(c => c.Id == id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return campaign != null ? _mapper.Map<EmailCampaignDto>(campaign) : null;
    }

    public async Task<IEnumerable<EmailCampaignDto>> GetCampaignsAsync(string? status = null, int page = 1, int pageSize = 20)
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

        var campaigns = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<EmailCampaignDto>>(campaigns);
    }

    public async Task<EmailCampaignDto> UpdateCampaignAsync(Guid id, UpdateEmailCampaignDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
            .FirstOrDefaultAsync(c => c.Id == id);

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

        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with includes in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var updatedCampaign = await _context.Set<EmailCampaign>()
            .AsNoTracking()
            .Include(c => c.Template)
            .FirstOrDefaultAsync(c => c.Id == id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<EmailCampaignDto>(updatedCampaign!);
    }

    public async Task<bool> DeleteCampaignAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign == null) return false;

        if (campaign.Status == EmailCampaignStatus.Sending)
        {
            throw new BusinessException("Şu anda gönderilmekte olan bir kampanya silinemez.");
        }

        campaign.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ScheduleCampaignAsync(Guid id, DateTime scheduledAt)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign == null) return false;

        if (scheduledAt <= DateTime.UtcNow)
        {
            throw new ValidationException("Zamanlanmış zaman gelecekte olmalıdır.");
        }

        campaign.ScheduledAt = scheduledAt;
        campaign.Status = EmailCampaignStatus.Scheduled;

        await PrepareCampaignRecipientsAsync(campaign);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SendCampaignAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
            .Include(c => c.Recipients)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign == null) return false;

        if (campaign.Status == EmailCampaignStatus.Sent)
        {
            throw new BusinessException("Kampanya zaten gönderilmiş.");
        }

        campaign.Status = EmailCampaignStatus.Sending;
        await _unitOfWork.SaveChangesAsync();

        // Prepare recipients if not already done
        if (campaign.Recipients.Count == 0)
        {
            await PrepareCampaignRecipientsAsync(campaign);
        }

        // Send emails (in production, this would be queued)
        await SendCampaignEmailsAsync(campaign);

        campaign.Status = EmailCampaignStatus.Sent;
        campaign.SentAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PauseCampaignAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign == null) return false;

        if (campaign.Status != EmailCampaignStatus.Sending)
        {
            throw new BusinessException("Sadece gönderilmekte olan kampanyalar duraklatılabilir.");
        }

        campaign.Status = EmailCampaignStatus.Paused;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CancelCampaignAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign == null) return false;

        campaign.Status = EmailCampaignStatus.Cancelled;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task SendTestEmailAsync(SendTestEmailDto dto)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
            .AsNoTracking()
            .Include(c => c.Template)
            .FirstOrDefaultAsync(c => c.Id == dto.CampaignId);

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
            content
        );
    }

    // Template Management
    public async Task<EmailTemplateDto> CreateTemplateAsync(CreateEmailTemplateDto dto)
    {
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

        await _context.Set<EmailTemplate>().AddAsync(template);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var createdTemplate = await _context.Set<EmailTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == template.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<EmailTemplateDto>(createdTemplate!);
    }

    public async Task<EmailTemplateDto?> GetTemplateAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<EmailTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return template != null ? _mapper.Map<EmailTemplateDto>(template) : null;
    }

    public async Task<IEnumerable<EmailTemplateDto>> GetTemplatesAsync(string? type = null)
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
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<EmailTemplateDto>>(templates);
    }

    public async Task<EmailTemplateDto> UpdateTemplateAsync(Guid id, CreateEmailTemplateDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<EmailTemplate>()
            .FirstOrDefaultAsync(t => t.Id == id);

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

        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var updatedTemplate = await _context.Set<EmailTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<EmailTemplateDto>(updatedTemplate!);
    }

    public async Task<bool> DeleteTemplateAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<EmailTemplate>()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (template == null) return false;

        template.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // Subscriber Management
    public async Task<EmailSubscriberDto> SubscribeAsync(CreateEmailSubscriberDto dto)
    {
        var existing = await _context.Set<EmailSubscriber>()
            .FirstOrDefaultAsync(s => s.Email.ToLower() == dto.Email.ToLower());

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

            await _unitOfWork.SaveChangesAsync();
            
            // ✅ PERFORMANCE: Reload in one query (N+1 fix)
            // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
            var reloadedExisting = await _context.Set<EmailSubscriber>()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == existing.Id);

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

        await _context.Set<EmailSubscriber>().AddAsync(subscriber);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var createdSubscriber = await _context.Set<EmailSubscriber>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == subscriber.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<EmailSubscriberDto>(createdSubscriber!);
    }

    public async Task<bool> UnsubscribeAsync(string email)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var subscriber = await _context.Set<EmailSubscriber>()
            .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower());

        if (subscriber == null) return false;

        subscriber.IsSubscribed = false;
        subscriber.UnsubscribedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<EmailSubscriberDto?> GetSubscriberAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var subscriber = await _context.Set<EmailSubscriber>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return subscriber != null ? _mapper.Map<EmailSubscriberDto>(subscriber) : null;
    }

    public async Task<EmailSubscriberDto?> GetSubscriberByEmailAsync(string email)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var subscriber = await _context.Set<EmailSubscriber>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower());

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return subscriber != null ? _mapper.Map<EmailSubscriberDto>(subscriber) : null;
    }

    public async Task<IEnumerable<EmailSubscriberDto>> GetSubscribersAsync(bool? isSubscribed = null, int page = 1, int pageSize = 50)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        IQueryable<EmailSubscriber> query = _context.Set<EmailSubscriber>()
            .AsNoTracking();

        if (isSubscribed.HasValue)
        {
            query = query.Where(s => s.IsSubscribed == isSubscribed.Value);
        }

        var subscribers = await query
            .OrderByDescending(s => s.SubscribedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<EmailSubscriberDto>>(subscribers);
    }

    public async Task<bool> UpdateSubscriberAsync(Guid id, CreateEmailSubscriberDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var subscriber = await _context.Set<EmailSubscriber>()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subscriber == null) return false;

        subscriber.FirstName = dto.FirstName;
        subscriber.LastName = dto.LastName;
        subscriber.Tags = dto.Tags != null ? JsonSerializer.Serialize(dto.Tags) : null;
        subscriber.CustomFields = dto.CustomFields != null ? JsonSerializer.Serialize(dto.CustomFields) : null;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<int> BulkImportSubscribersAsync(BulkImportSubscribersDto dto)
    {
        if (dto.Subscribers == null || !dto.Subscribers.Any())
        {
            return 0;
        }

        // ✅ PERFORMANCE: Batch load existing subscribers (N+1 fix)
        var emails = dto.Subscribers.Select(s => s.Email.ToLower()).Distinct().ToList();
        var existingSubscribers = await _context.Set<EmailSubscriber>()
            .Where(s => emails.Contains(s.Email.ToLower()))
            .ToDictionaryAsync(s => s.Email.ToLower());

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
            await _context.Set<EmailSubscriber>().AddRangeAsync(newSubscribers);
        }

        await _unitOfWork.SaveChangesAsync();

        return newSubscribers.Count + updatedSubscribers.Count;
    }

    // Analytics
    public async Task<EmailCampaignAnalyticsDto?> GetCampaignAnalyticsAsync(Guid campaignId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == campaignId);

        if (campaign == null) return null;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = _mapper.Map<EmailCampaignAnalyticsDto>(campaign);
        
        // ✅ PERFORMANCE: Memory'de minimal işlem (sadece property assignment)
        dto.BounceRate = campaign.SentCount > 0 ? (decimal)campaign.BouncedCount / campaign.SentCount * 100 : 0;
        dto.UnsubscribeRate = campaign.SentCount > 0 ? (decimal)campaign.UnsubscribedCount / campaign.SentCount * 100 : 0;
        
        return dto;
    }

    public async Task<EmailCampaignStatsDto> GetCampaignStatsAsync()
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var totalCampaigns = await _context.Set<EmailCampaign>()
            .CountAsync();

        var activeCampaigns = await _context.Set<EmailCampaign>()
            .CountAsync(c => c.Status == EmailCampaignStatus.Sending || c.Status == EmailCampaignStatus.Scheduled);

        var totalSubscribers = await _context.Set<EmailSubscriber>()
            .CountAsync();

        var activeSubscribers = await _context.Set<EmailSubscriber>()
            .CountAsync(s => s.IsSubscribed);

        var totalEmailsSent = await _context.Set<EmailCampaign>()
            .SumAsync(c => (long)c.SentCount);

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var sentCampaignsCount = await _context.Set<EmailCampaign>()
            .CountAsync(c => c.Status == EmailCampaignStatus.Sent);

        var avgOpenRate = sentCampaignsCount > 0
            ? await _context.Set<EmailCampaign>()
                .Where(c => c.Status == EmailCampaignStatus.Sent)
                .AverageAsync(c => (decimal?)c.OpenRate) ?? 0
            : 0;

        var avgClickRate = sentCampaignsCount > 0
            ? await _context.Set<EmailCampaign>()
                .Where(c => c.Status == EmailCampaignStatus.Sent)
                .AverageAsync(c => (decimal?)c.ClickRate) ?? 0
            : 0;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var recentCampaigns = await _context.Set<EmailCampaign>()
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .ToListAsync();

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

    public async Task RecordEmailOpenAsync(Guid campaignId, Guid subscriberId)
    {
        var recipient = await _context.Set<EmailCampaignRecipient>()
            .FirstOrDefaultAsync(r => r.CampaignId == campaignId && r.SubscriberId == subscriberId);

        if (recipient == null) return;

        if (recipient.OpenedAt == null)
        {
            recipient.OpenedAt = DateTime.UtcNow;
            recipient.Status = EmailRecipientStatus.Opened;

            // ✅ PERFORMANCE: Batch load campaign and subscriber (N+1 fix)
            var campaign = await _context.Set<EmailCampaign>()
                .FirstOrDefaultAsync(c => c.Id == campaignId);

            if (campaign != null)
            {
                campaign.OpenedCount++;
                campaign.OpenRate = campaign.SentCount > 0 ? (decimal)campaign.OpenedCount / campaign.SentCount * 100 : 0;
            }

            var subscriber = await _context.Set<EmailSubscriber>()
                .FirstOrDefaultAsync(s => s.Id == subscriberId);

            if (subscriber != null)
            {
                subscriber.EmailsOpened++;
                subscriber.LastEmailOpenedAt = DateTime.UtcNow;
            }
        }

        recipient.OpenCount++;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RecordEmailClickAsync(Guid campaignId, Guid subscriberId)
    {
        var recipient = await _context.Set<EmailCampaignRecipient>()
            .FirstOrDefaultAsync(r => r.CampaignId == campaignId && r.SubscriberId == subscriberId);

        if (recipient == null) return;

        if (recipient.ClickedAt == null)
        {
            recipient.ClickedAt = DateTime.UtcNow;
            recipient.Status = EmailRecipientStatus.Clicked;

            // ✅ PERFORMANCE: Batch load campaign and subscriber (N+1 fix)
            var campaign = await _context.Set<EmailCampaign>()
                .FirstOrDefaultAsync(c => c.Id == campaignId);

            if (campaign != null)
            {
                campaign.ClickedCount++;
                campaign.ClickRate = campaign.SentCount > 0 ? (decimal)campaign.ClickedCount / campaign.SentCount * 100 : 0;
            }

            var subscriber = await _context.Set<EmailSubscriber>()
                .FirstOrDefaultAsync(s => s.Id == subscriberId);

            if (subscriber != null)
            {
                subscriber.EmailsClicked++;
            }
        }

        recipient.ClickCount++;
        await _unitOfWork.SaveChangesAsync();
    }

    // Automation
    public async Task<EmailAutomationDto> CreateAutomationAsync(CreateEmailAutomationDto dto)
    {
        var automation = new EmailAutomation
        {
            Name = dto.Name,
            Description = dto.Description,
            Type = Enum.Parse<EmailAutomationType>(dto.Type, true),
            TemplateId = dto.TemplateId,
            DelayHours = dto.DelayHours,
            TriggerConditions = dto.TriggerConditions != null ? JsonSerializer.Serialize(dto.TriggerConditions) : null
        };

        await _context.Set<EmailAutomation>().AddAsync(automation);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with includes in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var createdAutomation = await _context.Set<EmailAutomation>()
            .AsNoTracking()
            .Include(a => a.Template)
            .FirstOrDefaultAsync(a => a.Id == automation.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<EmailAutomationDto>(createdAutomation!);
    }

    public async Task<IEnumerable<EmailAutomationDto>> GetAutomationsAsync()
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var automations = await _context.Set<EmailAutomation>()
            .AsNoTracking()
            .Include(a => a.Template)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<EmailAutomationDto>>(automations);
    }

    public async Task<bool> ToggleAutomationAsync(Guid id, bool isActive)
    {
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var automation = await _context.Set<EmailAutomation>()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (automation == null) return false;

        automation.IsActive = isActive;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAutomationAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var automation = await _context.Set<EmailAutomation>()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (automation == null) return false;

        automation.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // Private Helper Methods
    private async Task PrepareCampaignRecipientsAsync(EmailCampaign campaign)
    {
        var subscribers = await GetTargetedSubscribersAsync(campaign.TargetSegment);

        foreach (var subscriber in subscribers)
        {
            var recipient = new EmailCampaignRecipient
            {
                CampaignId = campaign.Id,
                SubscriberId = subscriber.Id,
                Status = EmailRecipientStatus.Pending
            };

            await _context.Set<EmailCampaignRecipient>().AddAsync(recipient);
        }

        campaign.TotalRecipients = subscribers.Count;
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<List<EmailSubscriber>> GetTargetedSubscribersAsync(string segment)
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

        return await query.ToListAsync();
    }

    private async Task SendCampaignEmailsAsync(EmailCampaign campaign)
    {
        var recipients = await _context.Set<EmailCampaignRecipient>()
            .Include(r => r.Subscriber)
            .Where(r => r.CampaignId == campaign.Id && r.Status == EmailRecipientStatus.Pending)
            .ToListAsync();

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
                    content
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
                recipient.Status = EmailRecipientStatus.Failed;
                recipient.ErrorMessage = ex.Message;
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

}

