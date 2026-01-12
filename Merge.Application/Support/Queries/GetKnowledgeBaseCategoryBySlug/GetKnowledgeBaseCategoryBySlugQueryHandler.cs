using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseCategoryBySlug;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetKnowledgeBaseCategoryBySlugQueryHandler : IRequestHandler<GetKnowledgeBaseCategoryBySlugQuery, KnowledgeBaseCategoryDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetKnowledgeBaseCategoryBySlugQueryHandler(
        IDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<KnowledgeBaseCategoryDto?> Handle(GetKnowledgeBaseCategoryBySlugQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için query splitting (Cartesian Explosion önleme)
        var category = await _context.Set<KnowledgeBaseCategory>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Slug == request.Slug && c.IsActive, cancellationToken);

        return category != null ? _mapper.Map<KnowledgeBaseCategoryDto>(category) : null;
    }
}
