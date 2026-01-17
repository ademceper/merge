using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetCustomerCommunication;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetCustomerCommunicationQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetCustomerCommunicationQuery, CustomerCommunicationDto?>
{

    public async Task<CustomerCommunicationDto?> Handle(GetCustomerCommunicationQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için query splitting (Cartesian Explosion önleme)
        // ✅ BOLUM 3.2: IDOR koruması - Kullanıcı sadece kendi communication'larına erişebilmeli
        var query = context.Set<CustomerCommunication>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.User)
            .Include(c => c.SentBy)
            .Where(c => c.Id == request.CommunicationId);

        // ✅ BOLUM 3.2: IDOR koruması - userId varsa filtrele
        if (request.UserId.HasValue)
        {
            query = query.Where(c => c.UserId == request.UserId.Value);
        }

        var communication = await query.FirstOrDefaultAsync(cancellationToken);

        return communication != null ? mapper.Map<CustomerCommunicationDto>(communication) : null;
    }
}
