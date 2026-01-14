using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetEmailSubscriberById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetEmailSubscriberByIdQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetEmailSubscriberByIdQuery, EmailSubscriberDto?>
{
    public async Task<EmailSubscriberDto?> Handle(GetEmailSubscriberByIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var subscriber = await context.Set<Merge.Domain.Modules.Marketing.EmailSubscriber>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return subscriber != null ? mapper.Map<EmailSubscriberDto>(subscriber) : null;
    }
}
