using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetFaq;

public class GetFaqQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetFaqQuery, FaqDto?>
{

    public async Task<FaqDto?> Handle(GetFaqQuery request, CancellationToken cancellationToken)
    {
        var faq = await context.Set<FAQ>()
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == request.FaqId, cancellationToken);
        
        return faq is null ? null : mapper.Map<FaqDto>(faq);
    }
}
