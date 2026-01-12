using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Support.Queries.GetFaq;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetFaqQueryHandler : IRequestHandler<GetFaqQuery, FaqDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetFaqQueryHandler(
        IDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FaqDto?> Handle(GetFaqQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !f.IsDeleted (Global Query Filter)
        var faq = await _context.Set<FAQ>()
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == request.FaqId, cancellationToken);
        
        return faq == null ? null : _mapper.Map<FaqDto>(faq);
    }
}
