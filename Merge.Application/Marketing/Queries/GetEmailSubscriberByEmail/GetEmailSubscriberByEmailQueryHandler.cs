using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;

namespace Merge.Application.Marketing.Queries.GetEmailSubscriberByEmail;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetEmailSubscriberByEmailQueryHandler : IRequestHandler<GetEmailSubscriberByEmailQuery, EmailSubscriberDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetEmailSubscriberByEmailQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<EmailSubscriberDto?> Handle(GetEmailSubscriberByEmailQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var subscriber = await _context.Set<Merge.Domain.Entities.EmailSubscriber>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return subscriber != null ? _mapper.Map<EmailSubscriberDto>(subscriber) : null;
    }
}
