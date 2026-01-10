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

namespace Merge.Application.Marketing.Commands.CreateEmailTemplate;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateEmailTemplateCommandHandler : IRequestHandler<CreateEmailTemplateCommand, EmailTemplateDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateEmailTemplateCommandHandler> _logger;

    public CreateEmailTemplateCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateEmailTemplateCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<EmailTemplateDto> Handle(CreateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email template oluşturuluyor. Name: {Name}, Type: {Type}",
            request.Name, request.Type);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var typeEnum = Enum.Parse<EmailTemplateType>(request.Type, true);
        var template = EmailTemplate.Create(
            name: request.Name,
            description: request.Description,
            subject: request.Subject,
            htmlContent: request.HtmlContent,
            textContent: request.TextContent,
            type: typeEnum,
            variables: request.Variables != null ? JsonSerializer.Serialize(request.Variables) : null,
            thumbnail: request.Thumbnail);

        await _context.Set<EmailTemplate>().AddAsync(template, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var createdTemplate = await _context.Set<EmailTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == template.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email template oluşturuldu. TemplateId: {TemplateId}, Name: {Name}",
            template.Id, request.Name);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<EmailTemplateDto>(createdTemplate!);
    }
}
