using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.CreateEmailCampaign;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class CreateEmailCampaignCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateEmailCampaignCommandHandler> logger) : IRequestHandler<CreateEmailCampaignCommand, EmailCampaignDto>
{

    public async Task<EmailCampaignDto> Handle(CreateEmailCampaignCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Email kampanyası oluşturuluyor. Name: {Name}, Type: {Type}, TargetSegment: {TargetSegment}",
            request.Name, request.Type, request.TargetSegment);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var campaign = EmailCampaign.Create(
            request.Name,
            request.Subject,
            request.FromName,
            request.FromEmail,
            request.ReplyToEmail,
            request.Content,
            Enum.Parse<EmailCampaignType>(request.Type, true),
            request.TargetSegment,
            request.ScheduledAt,
            request.TemplateId,
            request.Tags != null ? JsonSerializer.Serialize(request.Tags) : null);

        await context.Set<EmailCampaign>().AddAsync(campaign, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with includes in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + AsSplitQuery + Removed manual !c.IsDeleted (Global Query Filter)
        var createdCampaign = await context.Set<EmailCampaign>()
            .AsNoTracking()
            .Include(c => c.Template)
            .FirstOrDefaultAsync(c => c.Id == campaign.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Email kampanyası oluşturuldu. CampaignId: {CampaignId}, Name: {Name}",
            campaign.Id, request.Name);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<EmailCampaignDto>(createdCampaign!);
    }
}
