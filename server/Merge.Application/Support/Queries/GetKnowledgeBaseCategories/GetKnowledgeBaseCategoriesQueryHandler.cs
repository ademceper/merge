using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseCategories;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetKnowledgeBaseCategoriesQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetKnowledgeBaseCategoriesQuery, IEnumerable<KnowledgeBaseCategoryDto>>
{

    public async Task<IEnumerable<KnowledgeBaseCategoryDto>> Handle(GetKnowledgeBaseCategoriesQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için query splitting (Cartesian Explosion önleme)
        IQueryable<KnowledgeBaseCategory> query = context.Set<KnowledgeBaseCategory>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.ParentCategory)
            .Where(c => c.IsActive);

        if (request.IncludeSubCategories)
        {
            query = query.Include(c => c.SubCategories);
        }

        var categories = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return mapper.Map<IEnumerable<KnowledgeBaseCategoryDto>>(categories);
    }
}
