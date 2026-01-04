using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Marketing;

public interface IEmailCampaignService
{
    // Campaign Management
    Task<EmailCampaignDto> CreateCampaignAsync(CreateEmailCampaignDto dto, CancellationToken cancellationToken = default);
    Task<EmailCampaignDto?> GetCampaignAsync(Guid id, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    Task<PagedResult<EmailCampaignDto>> GetCampaignsAsync(string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<EmailCampaignDto> UpdateCampaignAsync(Guid id, UpdateEmailCampaignDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteCampaignAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ScheduleCampaignAsync(Guid id, DateTime scheduledAt, CancellationToken cancellationToken = default);
    Task<bool> SendCampaignAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> PauseCampaignAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CancelCampaignAsync(Guid id, CancellationToken cancellationToken = default);
    Task SendTestEmailAsync(SendTestEmailDto dto, CancellationToken cancellationToken = default);

    // Template Management
    Task<EmailTemplateDto> CreateTemplateAsync(CreateEmailTemplateDto dto, CancellationToken cancellationToken = default);
    Task<EmailTemplateDto?> GetTemplateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<EmailTemplateDto>> GetTemplatesAsync(string? type = null, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    Task<PagedResult<EmailTemplateDto>> GetTemplatesAsync(string? type, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<EmailTemplateDto> UpdateTemplateAsync(Guid id, CreateEmailTemplateDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteTemplateAsync(Guid id, CancellationToken cancellationToken = default);

    // Subscriber Management
    Task<EmailSubscriberDto> SubscribeAsync(CreateEmailSubscriberDto dto, CancellationToken cancellationToken = default);
    Task<bool> UnsubscribeAsync(string email, CancellationToken cancellationToken = default);
    Task<EmailSubscriberDto?> GetSubscriberAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmailSubscriberDto?> GetSubscriberByEmailAsync(string email, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    Task<PagedResult<EmailSubscriberDto>> GetSubscribersAsync(bool? isSubscribed = null, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<bool> UpdateSubscriberAsync(Guid id, CreateEmailSubscriberDto dto, CancellationToken cancellationToken = default);
    Task<int> BulkImportSubscribersAsync(BulkImportSubscribersDto dto, CancellationToken cancellationToken = default);

    // Analytics
    Task<EmailCampaignAnalyticsDto?> GetCampaignAnalyticsAsync(Guid campaignId, CancellationToken cancellationToken = default);
    Task<EmailCampaignStatsDto> GetCampaignStatsAsync(CancellationToken cancellationToken = default);
    Task RecordEmailOpenAsync(Guid campaignId, Guid subscriberId, CancellationToken cancellationToken = default);
    Task RecordEmailClickAsync(Guid campaignId, Guid subscriberId, CancellationToken cancellationToken = default);

    // Automation
    Task<EmailAutomationDto> CreateAutomationAsync(CreateEmailAutomationDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<EmailAutomationDto>> GetAutomationsAsync(CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    Task<PagedResult<EmailAutomationDto>> GetAutomationsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> ToggleAutomationAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);
    Task<bool> DeleteAutomationAsync(Guid id, CancellationToken cancellationToken = default);
}
