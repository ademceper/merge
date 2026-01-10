using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;

namespace Merge.Application.Marketing.Commands.CreateEmailAutomation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateEmailAutomationCommandHandler : IRequestHandler<CreateEmailAutomationCommand, EmailAutomationDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateEmailAutomationCommandHandler> _logger;

    public CreateEmailAutomationCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateEmailAutomationCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<EmailAutomationDto> Handle(CreateEmailAutomationCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email otomasyonu oluşturuluyor. Name: {Name}, Type: {Type}",
            request.Name, request.Type);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var typeEnum = Enum.Parse<EmailAutomationType>(request.Type, true);
        var automation = EmailAutomation.Create(
            name: request.Name,
            description: request.Description,
            type: typeEnum,
            templateId: request.TemplateId,
            delayHours: request.DelayHours,
            triggerConditions: request.TriggerConditions != null ? JsonSerializer.Serialize(request.TriggerConditions) : null);

        await _context.Set<EmailAutomation>().AddAsync(automation, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with includes in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + AsSplitQuery + Removed manual !a.IsDeleted (Global Query Filter)
        var createdAutomation = await _context.Set<EmailAutomation>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Template)
            .FirstOrDefaultAsync(a => a.Id == automation.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email otomasyonu oluşturuldu. AutomationId: {AutomationId}, Name: {Name}",
            automation.Id, request.Name);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<EmailAutomationDto>(createdAutomation!);
    }
}
