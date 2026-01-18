using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using EmailTemplate = Merge.Domain.Modules.Notifications.EmailTemplate;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetEmailTemplateById;

public class GetEmailTemplateByIdQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetEmailTemplateByIdQuery, EmailTemplateDto?>
{
    public async Task<EmailTemplateDto?> Handle(GetEmailTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await context.Set<EmailTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        return template is not null ? mapper.Map<EmailTemplateDto>(template) : null;
    }
}
