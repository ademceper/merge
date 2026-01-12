using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetEmailTemplateById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetEmailTemplateByIdQueryHandler : IRequestHandler<GetEmailTemplateByIdQuery, EmailTemplateDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetEmailTemplateByIdQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<EmailTemplateDto?> Handle(GetEmailTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<Merge.Domain.Modules.Notifications.EmailTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return template != null ? _mapper.Map<EmailTemplateDto>(template) : null;
    }
}
