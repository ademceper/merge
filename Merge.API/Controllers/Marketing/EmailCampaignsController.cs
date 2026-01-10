using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Marketing.Commands.CreateEmailCampaign;
using Merge.Application.Marketing.Queries.GetEmailCampaignById;
using Merge.Application.Marketing.Queries.GetAllEmailCampaigns;
using Merge.Application.Marketing.Commands.UpdateEmailCampaign;
using Merge.Application.Marketing.Commands.DeleteEmailCampaign;
using Merge.Application.Marketing.Commands.ScheduleEmailCampaign;
using Merge.Application.Marketing.Commands.SendEmailCampaign;
using Merge.Application.Marketing.Commands.PauseEmailCampaign;
using Merge.Application.Marketing.Commands.CancelEmailCampaign;
using Merge.Application.Marketing.Commands.SendTestEmail;
using Merge.Application.Marketing.Commands.CreateEmailTemplate;
using Merge.Application.Marketing.Queries.GetEmailTemplateById;
using Merge.Application.Marketing.Queries.GetAllEmailTemplates;
using Merge.Application.Marketing.Commands.UpdateEmailTemplate;
using Merge.Application.Marketing.Commands.DeleteEmailTemplate;
using Merge.Application.Marketing.Commands.SubscribeEmail;
using Merge.Application.Marketing.Commands.UnsubscribeEmail;
using Merge.Application.Marketing.Queries.GetEmailSubscriberById;
using Merge.Application.Marketing.Queries.GetEmailSubscriberByEmail;
using Merge.Application.Marketing.Queries.GetAllEmailSubscribers;
using Merge.Application.Marketing.Commands.UpdateEmailSubscriber;
using Merge.Application.Marketing.Commands.BulkImportEmailSubscribers;
using Merge.Application.Marketing.Queries.GetCampaignAnalytics;
using Merge.Application.Marketing.Queries.GetCampaignStats;
using Merge.Application.Marketing.Commands.RecordEmailOpen;
using Merge.Application.Marketing.Commands.RecordEmailClick;
using Merge.Application.Marketing.Commands.CreateEmailAutomation;
using Merge.Application.Marketing.Queries.GetAllEmailAutomations;
using Merge.Application.Marketing.Commands.ToggleEmailAutomation;
using Merge.Application.Marketing.Commands.DeleteEmailAutomation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.API.Controllers.Marketing;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/marketing/email-campaigns")]
[Authorize]
public class EmailCampaignsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly MarketingSettings _marketingSettings;

    public EmailCampaignsController(
        IMediator mediator,
        IOptions<MarketingSettings> marketingSettings)
    {
        _mediator = mediator;
        _marketingSettings = marketingSettings.Value;
    }

    // Campaign Management
    /// <summary>
    /// Yeni email kampanyası oluşturur (Admin, Manager)
    /// </summary>
    /// <param name="dto">Kampanya oluşturma verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan kampanya</returns>
    /// <response code="201">Kampanya başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [SwaggerOperation(
        Summary = "Yeni email kampanyası oluşturur",
        Description = "Admin veya Manager rolüne sahip kullanıcılar yeni email kampanyası oluşturabilir.")]
    [ProducesResponseType(typeof(EmailCampaignDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailCampaignDto>> CreateCampaign(
        [FromBody] CreateEmailCampaignDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new CreateEmailCampaignCommand(
            dto.Name,
            dto.Subject,
            dto.FromName,
            dto.FromEmail,
            dto.ReplyToEmail,
            dto.TemplateId,
            dto.Content,
            dto.Type ?? "Promotional",
            dto.ScheduledAt,
            dto.TargetSegment ?? "All",
            dto.Tags);

        var campaign = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCampaign), new { id = campaign.Id }, campaign);
    }

    /// <summary>
    /// Email kampanyası detaylarını getirir (Admin, Manager)
    /// </summary>
    /// <param name="id">Kampanya ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kampanya detayları</returns>
    /// <response code="200">Kampanya başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="404">Kampanya bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Email kampanyası detaylarını getirir",
        Description = "Admin veya Manager rolüne sahip kullanıcılar kampanya detaylarını görüntüleyebilir.")]
    [ProducesResponseType(typeof(EmailCampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailCampaignDto>> GetCampaign(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetEmailCampaignByIdQuery(id);
        var campaign = await _mediator.Send(query, cancellationToken);

        if (campaign == null)
        {
            return NotFound();
        }

        return Ok(campaign);
    }

    /// <summary>
    /// Email kampanyalarını getirir (pagination ile) (Admin, Manager)
    /// </summary>
    /// <param name="status">Kampanya durumu (opsiyonel)</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı (varsayılan: 20, maksimum: 100)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kampanya listesi</returns>
    /// <response code="200">Kampanyalar başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Email kampanyalarını getirir",
        Description = "Sayfalama ile email kampanyalarını getirir. Status parametresi ile filtreleme yapılabilir.")]
    [ProducesResponseType(typeof(PagedResult<EmailCampaignDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<EmailCampaignDto>>> GetCampaigns(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        if (pageSize > _marketingSettings.MaxPageSize) pageSize = _marketingSettings.MaxPageSize;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetAllEmailCampaignsQuery(status, PageNumber: page, PageSize: pageSize);
        var campaigns = await _mediator.Send(query, cancellationToken);
        return Ok(campaigns);
    }

    /// <summary>
    /// Email kampanyası bilgilerini günceller (Admin, Manager)
    /// </summary>
    /// <param name="id">Kampanya ID'si</param>
    /// <param name="dto">Güncelleme verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Güncellenmiş kampanya</returns>
    /// <response code="200">Kampanya başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="404">Kampanya bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [SwaggerOperation(
        Summary = "Email kampanyası bilgilerini günceller",
        Description = "Sadece taslak durumundaki kampanyalar güncellenebilir.")]
    [ProducesResponseType(typeof(EmailCampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailCampaignDto>> UpdateCampaign(
        Guid id,
        [FromBody] UpdateEmailCampaignDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new UpdateEmailCampaignCommand(
            id,
            dto.Name,
            dto.Subject,
            dto.FromName,
            dto.FromEmail,
            dto.ReplyToEmail,
            dto.TemplateId,
            dto.Content,
            dto.ScheduledAt,
            dto.TargetSegment);

        var campaign = await _mediator.Send(command, cancellationToken);
        return Ok(campaign);
    }

    /// <summary>
    /// Email kampanyasını siler (Admin, Manager)
    /// </summary>
    /// <param name="id">Kampanya ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Silme işlemi sonucu</returns>
    /// <response code="204">Kampanya başarıyla silindi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="404">Kampanya bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [SwaggerOperation(
        Summary = "Email kampanyasını siler",
        Description = "Gönderilmekte olan kampanyalar silinemez. Soft delete işlemi yapılır.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteCampaign(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new DeleteEmailCampaignCommand(id);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Email kampanyasını zamanlar (Admin, Manager)
    /// </summary>
    /// <param name="id">Kampanya ID'si</param>
    /// <param name="scheduledAt">Planlanan tarih</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Zamanlama işlemi sonucu</returns>
    /// <response code="204">Kampanya başarıyla zamanlandı</response>
    /// <response code="400">Geçersiz tarih</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="404">Kampanya bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("{id}/schedule")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [SwaggerOperation(
        Summary = "Email kampanyasını zamanlar",
        Description = "Kampanyayı belirtilen tarihte gönderilmek üzere zamanlar.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ScheduleCampaign(
        Guid id,
        [FromBody] DateTime scheduledAt,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new ScheduleEmailCampaignCommand(id, scheduledAt);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Email kampanyasını gönderir (Admin, Manager)
    /// </summary>
    /// <param name="id">Kampanya ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Gönderme işlemi sonucu</returns>
    /// <response code="204">Kampanya başarıyla gönderildi</response>
    /// <response code="400">Kampanya zaten gönderilmiş</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="404">Kampanya bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("{id}/send")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika (kritik işlem)
    [SwaggerOperation(
        Summary = "Email kampanyasını gönderir",
        Description = "Kampanyayı hemen gönderir. Production'da bu işlem queue'ya alınmalıdır.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendCampaign(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new SendEmailCampaignCommand(id);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Email kampanyasını duraklatır (Admin, Manager)
    /// </summary>
    /// <param name="id">Kampanya ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Duraklatma işlemi sonucu</returns>
    /// <response code="204">Kampanya başarıyla duraklatıldı</response>
    /// <response code="400">Sadece gönderilmekte olan kampanyalar duraklatılabilir</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="404">Kampanya bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("{id}/pause")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [SwaggerOperation(
        Summary = "Email kampanyasını duraklatır",
        Description = "Sadece gönderilmekte olan kampanyalar duraklatılabilir.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PauseCampaign(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new PauseEmailCampaignCommand(id);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Email kampanyasını iptal eder (Admin, Manager)
    /// </summary>
    /// <param name="id">Kampanya ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İptal işlemi sonucu</returns>
    /// <response code="204">Kampanya başarıyla iptal edildi</response>
    /// <response code="400">Gönderilmiş kampanyalar iptal edilemez</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="404">Kampanya bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [SwaggerOperation(
        Summary = "Email kampanyasını iptal eder",
        Description = "Gönderilmiş kampanyalar iptal edilemez.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CancelCampaign(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new CancelEmailCampaignCommand(id);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Test email gönderir (Admin, Manager)
    /// </summary>
    /// <param name="dto">Test email verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Test email gönderme işlemi sonucu</returns>
    /// <response code="204">Test email başarıyla gönderildi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="404">Kampanya bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("test-email")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [SwaggerOperation(
        Summary = "Test email gönderir",
        Description = "Kampanyanın test email'ini belirtilen adrese gönderir.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendTestEmail(
        [FromBody] SendTestEmailDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new SendTestEmailCommand(dto.CampaignId, dto.TestEmail);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    // Template Management
    /// <summary>
    /// Yeni email şablonu oluşturur (Admin, Manager)
    /// </summary>
    /// <param name="dto">Şablon oluşturma verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan şablon</returns>
    /// <response code="201">Şablon başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("templates")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [SwaggerOperation(
        Summary = "Yeni email şablonu oluşturur",
        Description = "Admin veya Manager rolüne sahip kullanıcılar yeni email şablonu oluşturabilir.")]
    [ProducesResponseType(typeof(EmailTemplateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailTemplateDto>> CreateTemplate(
        [FromBody] CreateEmailTemplateDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new CreateEmailTemplateCommand(
            dto.Name,
            dto.Description ?? string.Empty,
            dto.Subject,
            dto.HtmlContent,
            dto.TextContent ?? string.Empty,
            dto.Type ?? "Custom",
            dto.Variables,
            dto.Thumbnail);

        var template = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, template);
    }

    /// <summary>
    /// Email şablonu detaylarını getirir (Admin, Manager)
    /// </summary>
    /// <param name="id">Şablon ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Şablon detayları</returns>
    /// <response code="200">Şablon başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="404">Şablon bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("templates/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Email şablonu detaylarını getirir",
        Description = "Admin veya Manager rolüne sahip kullanıcılar şablon detaylarını görüntüleyebilir.")]
    [ProducesResponseType(typeof(EmailTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailTemplateDto>> GetTemplate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetEmailTemplateByIdQuery(id);
        var template = await _mediator.Send(query, cancellationToken);

        if (template == null)
        {
            return NotFound();
        }

        return Ok(template);
    }

    /// <summary>
    /// Email şablonlarını getirir (pagination ile) (Admin, Manager)
    /// </summary>
    /// <param name="type">Şablon tipi (opsiyonel)</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı (varsayılan: 20, maksimum: 100)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Şablon listesi</returns>
    /// <response code="200">Şablonlar başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("templates")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Email şablonlarını getirir",
        Description = "Sayfalama ile email şablonlarını getirir. Type parametresi ile filtreleme yapılabilir.")]
    [ProducesResponseType(typeof(PagedResult<EmailTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<EmailTemplateDto>>> GetTemplates(
        [FromQuery] string? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        if (pageSize > _marketingSettings.MaxPageSize) pageSize = _marketingSettings.MaxPageSize;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetAllEmailTemplatesQuery(type, PageNumber: page, PageSize: pageSize);
        var templates = await _mediator.Send(query, cancellationToken);
        return Ok(templates);
    }

    /// <summary>
    /// Email şablonu bilgilerini günceller (Admin, Manager)
    /// </summary>
    /// <param name="id">Şablon ID'si</param>
    /// <param name="dto">Güncelleme verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Güncellenmiş şablon</returns>
    /// <response code="200">Şablon başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="404">Şablon bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPut("templates/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [SwaggerOperation(
        Summary = "Email şablonu bilgilerini günceller",
        Description = "Admin veya Manager rolüne sahip kullanıcılar şablon bilgilerini güncelleyebilir.")]
    [ProducesResponseType(typeof(EmailTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailTemplateDto>> UpdateTemplate(
        Guid id,
        [FromBody] CreateEmailTemplateDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new UpdateEmailTemplateCommand(
            id,
            dto.Name,
            dto.Description,
            dto.Subject,
            dto.HtmlContent,
            dto.TextContent,
            dto.Type,
            dto.Variables,
            dto.Thumbnail,
            null); // IsActive is optional, can be set separately via Activate/Deactivate endpoints

        var template = await _mediator.Send(command, cancellationToken);
        return Ok(template);
    }

    /// <summary>
    /// Email şablonunu siler (Admin, Manager)
    /// </summary>
    /// <param name="id">Şablon ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Silme işlemi sonucu</returns>
    /// <response code="204">Şablon başarıyla silindi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="404">Şablon bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpDelete("templates/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [SwaggerOperation(
        Summary = "Email şablonunu siler",
        Description = "Admin veya Manager rolüne sahip kullanıcılar şablonu silebilir. Soft delete işlemi yapılır.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteTemplate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new DeleteEmailTemplateCommand(id);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    // Subscriber Management
    /// <summary>
    /// Email aboneliği oluşturur veya günceller
    /// </summary>
    /// <param name="dto">Abonelik verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan veya güncellenen abone</returns>
    /// <response code="200">Abonelik başarıyla oluşturuldu veya güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("subscribers")]
    [AllowAnonymous]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [SwaggerOperation(
        Summary = "Email aboneliği oluşturur veya günceller",
        Description = "Email aboneliği oluşturur. Eğer email zaten varsa güncellenir.")]
    [ProducesResponseType(typeof(EmailSubscriberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailSubscriberDto>> Subscribe(
        [FromBody] CreateEmailSubscriberDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new SubscribeEmailCommand(
            dto.Email,
            dto.FirstName,
            dto.LastName,
            dto.Source,
            dto.Tags,
            dto.CustomFields);

        var subscriber = await _mediator.Send(command, cancellationToken);
        return Ok(subscriber);
    }

    /// <summary>
    /// Email aboneliğini iptal eder
    /// </summary>
    /// <param name="email">E-posta adresi</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İptal işlemi sonucu</returns>
    /// <response code="204">Abonelik başarıyla iptal edildi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="404">Abone bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("subscribers/unsubscribe")]
    [AllowAnonymous]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [SwaggerOperation(
        Summary = "Email aboneliğini iptal eder",
        Description = "Email aboneliğini iptal eder. Herkese açık endpoint.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Unsubscribe(
        [FromBody] string email,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new UnsubscribeEmailCommand(email);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Email abonesi detaylarını getirir (Admin, Manager)
    /// </summary>
    /// <param name="id">Abone ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Abone detayları</returns>
    /// <response code="200">Abone başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="404">Abone bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("subscribers/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Email abonesi detaylarını getirir",
        Description = "Admin veya Manager rolüne sahip kullanıcılar abone detaylarını görüntüleyebilir.")]
    [ProducesResponseType(typeof(EmailSubscriberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailSubscriberDto>> GetSubscriber(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetEmailSubscriberByIdQuery(id);
        var subscriber = await _mediator.Send(query, cancellationToken);

        if (subscriber == null)
        {
            return NotFound();
        }

        return Ok(subscriber);
    }

    /// <summary>
    /// Email abonesi detaylarını email ile getirir (Admin, Manager)
    /// </summary>
    /// <param name="email">E-posta adresi</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Abone detayları</returns>
    /// <response code="200">Abone başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="404">Abone bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("subscribers/by-email/{email}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Email abonesi detaylarını email ile getirir",
        Description = "Admin veya Manager rolüne sahip kullanıcılar email ile abone detaylarını görüntüleyebilir.")]
    [ProducesResponseType(typeof(EmailSubscriberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailSubscriberDto>> GetSubscriberByEmail(
        string email,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetEmailSubscriberByEmailQuery(email);
        var subscriber = await _mediator.Send(query, cancellationToken);

        if (subscriber == null)
        {
            return NotFound();
        }

        return Ok(subscriber);
    }

    /// <summary>
    /// Email abonelerini getirir (pagination ile) (Admin, Manager)
    /// </summary>
    /// <param name="isSubscribed">Abonelik durumu (opsiyonel)</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı (varsayılan: 50, maksimum: 100)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Abone listesi</returns>
    /// <response code="200">Aboneler başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("subscribers")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Email abonelerini getirir",
        Description = "Sayfalama ile email abonelerini getirir. IsSubscribed parametresi ile filtreleme yapılabilir.")]
    [ProducesResponseType(typeof(PagedResult<EmailSubscriberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<EmailSubscriberDto>>> GetSubscribers(
        [FromQuery] bool? isSubscribed = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        if (pageSize > _marketingSettings.MaxPageSize) pageSize = _marketingSettings.MaxPageSize;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetAllEmailSubscribersQuery(isSubscribed, PageNumber: page, PageSize: pageSize);
        var subscribers = await _mediator.Send(query, cancellationToken);
        return Ok(subscribers);
    }

    /// <summary>
    /// Email abonesi bilgilerini günceller (Admin, Manager)
    /// </summary>
    /// <param name="id">Abone ID'si</param>
    /// <param name="dto">Güncelleme verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Güncelleme işlemi sonucu</returns>
    /// <response code="204">Abone başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="404">Abone bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPut("subscribers/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [SwaggerOperation(
        Summary = "Email abonesi bilgilerini günceller",
        Description = "Admin veya Manager rolüne sahip kullanıcılar abone bilgilerini güncelleyebilir.")]
    [ProducesResponseType(typeof(EmailSubscriberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailSubscriberDto>> UpdateSubscriber(
        Guid id,
        [FromBody] UpdateEmailSubscriberDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new UpdateEmailSubscriberCommand(
            id,
            dto.FirstName,
            dto.LastName,
            dto.Source,
            dto.Tags,
            dto.CustomFields,
            dto.IsSubscribed);

        var subscriber = await _mediator.Send(command, cancellationToken);
        return Ok(subscriber);
    }

    /// <summary>
    /// Toplu email abone import işlemi yapar (Admin, Manager)
    /// </summary>
    /// <param name="dto">Toplu import verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Import edilen abone sayısı</returns>
    /// <response code="200">Toplu import işlemi başarıyla tamamlandı</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("subscribers/bulk-import")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika (kritik işlem)
    [SwaggerOperation(
        Summary = "Toplu email abone import işlemi yapar",
        Description = "Admin veya Manager rolüne sahip kullanıcılar toplu abone import işlemi yapabilir. Bir seferde en fazla 1000 abone import edilebilir.")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<int>> BulkImportSubscribers(
        [FromBody] BulkImportSubscribersDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new BulkImportEmailSubscribersCommand(dto.Subscribers);
        var count = await _mediator.Send(command, cancellationToken);
        return Ok(count);
    }

    // Analytics
    /// <summary>
    /// Email kampanyası analitik verilerini getirir (Admin, Manager)
    /// </summary>
    /// <param name="campaignId">Kampanya ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kampanya analitik verileri</returns>
    /// <response code="200">Analitik veriler başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="404">Kampanya bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("{campaignId}/analytics")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Email kampanyası analitik verilerini getirir",
        Description = "Admin veya Manager rolüne sahip kullanıcılar kampanya analitik verilerini görüntüleyebilir.")]
    [ProducesResponseType(typeof(EmailCampaignAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailCampaignAnalyticsDto>> GetCampaignAnalytics(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetCampaignAnalyticsQuery(campaignId);
        var analytics = await _mediator.Send(query, cancellationToken);

        if (analytics == null)
        {
            return NotFound();
        }

        return Ok(analytics);
    }

    /// <summary>
    /// Email kampanya istatistiklerini getirir (Admin, Manager)
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kampanya istatistikleri</returns>
    /// <response code="200">İstatistikler başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Email kampanya istatistiklerini getirir",
        Description = "Admin veya Manager rolüne sahip kullanıcılar genel kampanya istatistiklerini görüntüleyebilir.")]
    [ProducesResponseType(typeof(EmailCampaignStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailCampaignStatsDto>> GetCampaignStats(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetCampaignStatsQuery();
        var stats = await _mediator.Send(query, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Email açılmasını kaydeder (herkese açık, tracking için)
    /// </summary>
    /// <param name="campaignId">Kampanya ID'si</param>
    /// <param name="subscriberId">Abone ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kayıt işlemi sonucu</returns>
    /// <response code="204">Email açılması başarıyla kaydedildi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("{campaignId}/subscribers/{subscriberId}/open")]
    [AllowAnonymous]
    [RateLimit(100, 60)] // ✅ BOLUM 3.3: Rate Limiting - 100/dakika (tracking endpoint)
    [SwaggerOperation(
        Summary = "Email açılmasını kaydeder",
        Description = "Email açılmasını kaydeder. Tracking için kullanılır, herkese açık endpoint.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RecordEmailOpen(
        Guid campaignId,
        Guid subscriberId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new RecordEmailOpenCommand(campaignId, subscriberId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Email tıklamasını kaydeder (herkese açık, tracking için)
    /// </summary>
    /// <param name="campaignId">Kampanya ID'si</param>
    /// <param name="subscriberId">Abone ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kayıt işlemi sonucu</returns>
    /// <response code="204">Email tıklaması başarıyla kaydedildi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("{campaignId}/subscribers/{subscriberId}/click")]
    [AllowAnonymous]
    [RateLimit(100, 60)] // ✅ BOLUM 3.3: Rate Limiting - 100/dakika (tracking endpoint)
    [SwaggerOperation(
        Summary = "Email tıklamasını kaydeder",
        Description = "Email tıklamasını kaydeder. Tracking için kullanılır, herkese açık endpoint.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RecordEmailClick(
        Guid campaignId,
        Guid subscriberId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new RecordEmailClickCommand(campaignId, subscriberId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    // Automation
    /// <summary>
    /// Yeni email otomasyonu oluşturur (Admin, Manager)
    /// </summary>
    /// <param name="dto">Otomasyon oluşturma verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan otomasyon</returns>
    /// <response code="201">Otomasyon başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("automations")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [SwaggerOperation(
        Summary = "Yeni email otomasyonu oluşturur",
        Description = "Admin veya Manager rolüne sahip kullanıcılar yeni email otomasyonu oluşturabilir.")]
    [ProducesResponseType(typeof(EmailAutomationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailAutomationDto>> CreateAutomation(
        [FromBody] CreateEmailAutomationDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new CreateEmailAutomationCommand(
            dto.Name,
            dto.Description ?? string.Empty,
            dto.Type,
            dto.TemplateId,
            dto.DelayHours,
            dto.TriggerConditions);

        var automation = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAutomations), new { page = 1, pageSize = 20 }, automation);
    }

    /// <summary>
    /// Email otomasyonlarını getirir (pagination ile) (Admin, Manager)
    /// </summary>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı (varsayılan: 20, maksimum: 100)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Otomasyon listesi</returns>
    /// <response code="200">Otomasyonlar başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("automations")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Email otomasyonlarını getirir",
        Description = "Sayfalama ile email otomasyonlarını getirir.")]
    [ProducesResponseType(typeof(PagedResult<EmailAutomationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<EmailAutomationDto>>> GetAutomations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        if (pageSize > _marketingSettings.MaxPageSize) pageSize = _marketingSettings.MaxPageSize;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetAllEmailAutomationsQuery(PageNumber: page, PageSize: pageSize);
        var automations = await _mediator.Send(query, cancellationToken);
        return Ok(automations);
    }

    /// <summary>
    /// Email otomasyonu durumunu değiştirir (Admin, Manager)
    /// </summary>
    /// <param name="id">Otomasyon ID'si</param>
    /// <param name="isActive">Aktif durumu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Durum değiştirme işlemi sonucu</returns>
    /// <response code="204">Otomasyon durumu başarıyla değiştirildi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="404">Otomasyon bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPatch("automations/{id}/toggle")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [SwaggerOperation(
        Summary = "Email otomasyonu durumunu değiştirir",
        Description = "Admin veya Manager rolüne sahip kullanıcılar otomasyon durumunu aktif/pasif yapabilir.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ToggleAutomation(
        Guid id,
        [FromBody] bool isActive,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new ToggleEmailAutomationCommand(id, isActive);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Email otomasyonunu siler (Admin, Manager)
    /// </summary>
    /// <param name="id">Otomasyon ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Silme işlemi sonucu</returns>
    /// <response code="204">Otomasyon başarıyla silindi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetkisiz erişim</response>
    /// <response code="404">Otomasyon bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpDelete("automations/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [SwaggerOperation(
        Summary = "Email otomasyonunu siler",
        Description = "Admin veya Manager rolüne sahip kullanıcılar otomasyonu silebilir. Soft delete işlemi yapılır.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteAutomation(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new DeleteEmailAutomationCommand(id);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}
