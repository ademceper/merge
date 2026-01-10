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

namespace Merge.Application.Marketing.Commands.CreateEmailCampaign;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateEmailCampaignCommandHandler : IRequestHandler<CreateEmailCampaignCommand, EmailCampaignDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateEmailCampaignCommandHandler> _logger;

    public CreateEmailCampaignCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateEmailCampaignCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<EmailCampaignDto> Handle(CreateEmailCampaignCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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

        await _context.Set<EmailCampaign>().AddAsync(campaign, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with includes in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + AsSplitQuery + Removed manual !c.IsDeleted (Global Query Filter)
        var createdCampaign = await _context.Set<EmailCampaign>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Template)
            .FirstOrDefaultAsync(c => c.Id == campaign.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email kampanyası oluşturuldu. CampaignId: {CampaignId}, Name: {Name}",
            campaign.Id, request.Name);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<EmailCampaignDto>(createdCampaign!);
    }
}
