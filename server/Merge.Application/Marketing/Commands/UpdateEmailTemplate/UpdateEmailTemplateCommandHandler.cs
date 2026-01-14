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
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.UpdateEmailTemplate;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class UpdateEmailTemplateCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateEmailTemplateCommandHandler> logger) : IRequestHandler<UpdateEmailTemplateCommand, EmailTemplateDto>
{
    public async Task<EmailTemplateDto> Handle(UpdateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var template = await context.Set<EmailTemplate>()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (template == null)
        {
            throw new NotFoundException("Şablon", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        EmailTemplateType? typeEnum = null;
        if (!string.IsNullOrEmpty(request.Type))
            typeEnum = Enum.Parse<EmailTemplateType>(request.Type, true);

        template.UpdateDetails(
            name: request.Name,
            description: request.Description,
            subject: request.Subject,
            htmlContent: request.HtmlContent,
            textContent: request.TextContent,
            type: typeEnum,
            variables: request.Variables != null ? JsonSerializer.Serialize(request.Variables) : null,
            thumbnail: request.Thumbnail);

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
                template.Activate();
            else
                template.Deactivate();
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var updatedTemplate = await context.Set<EmailTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<EmailTemplateDto>(updatedTemplate!);
    }
}
