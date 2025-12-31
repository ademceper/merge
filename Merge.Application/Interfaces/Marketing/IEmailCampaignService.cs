using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Interfaces.Marketing;

public interface IEmailCampaignService
{
    // Campaign Management
    Task<EmailCampaignDto> CreateCampaignAsync(CreateEmailCampaignDto dto);
    Task<EmailCampaignDto?> GetCampaignAsync(Guid id);
    Task<IEnumerable<EmailCampaignDto>> GetCampaignsAsync(string? status = null, int page = 1, int pageSize = 20);
    Task<EmailCampaignDto> UpdateCampaignAsync(Guid id, UpdateEmailCampaignDto dto);
    Task<bool> DeleteCampaignAsync(Guid id);
    Task<bool> ScheduleCampaignAsync(Guid id, DateTime scheduledAt);
    Task<bool> SendCampaignAsync(Guid id);
    Task<bool> PauseCampaignAsync(Guid id);
    Task<bool> CancelCampaignAsync(Guid id);
    Task SendTestEmailAsync(SendTestEmailDto dto);

    // Template Management
    Task<EmailTemplateDto> CreateTemplateAsync(CreateEmailTemplateDto dto);
    Task<EmailTemplateDto?> GetTemplateAsync(Guid id);
    Task<IEnumerable<EmailTemplateDto>> GetTemplatesAsync(string? type = null);
    Task<EmailTemplateDto> UpdateTemplateAsync(Guid id, CreateEmailTemplateDto dto);
    Task<bool> DeleteTemplateAsync(Guid id);

    // Subscriber Management
    Task<EmailSubscriberDto> SubscribeAsync(CreateEmailSubscriberDto dto);
    Task<bool> UnsubscribeAsync(string email);
    Task<EmailSubscriberDto?> GetSubscriberAsync(Guid id);
    Task<EmailSubscriberDto?> GetSubscriberByEmailAsync(string email);
    Task<IEnumerable<EmailSubscriberDto>> GetSubscribersAsync(bool? isSubscribed = null, int page = 1, int pageSize = 50);
    Task<bool> UpdateSubscriberAsync(Guid id, CreateEmailSubscriberDto dto);
    Task<int> BulkImportSubscribersAsync(BulkImportSubscribersDto dto);

    // Analytics
    Task<EmailCampaignAnalyticsDto?> GetCampaignAnalyticsAsync(Guid campaignId);
    Task<EmailCampaignStatsDto> GetCampaignStatsAsync();
    Task RecordEmailOpenAsync(Guid campaignId, Guid subscriberId);
    Task RecordEmailClickAsync(Guid campaignId, Guid subscriberId);

    // Automation
    Task<EmailAutomationDto> CreateAutomationAsync(CreateEmailAutomationDto dto);
    Task<IEnumerable<EmailAutomationDto>> GetAutomationsAsync();
    Task<bool> ToggleAutomationAsync(Guid id, bool isActive);
    Task<bool> DeleteAutomationAsync(Guid id);
}
