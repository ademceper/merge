using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseCategoryBySlug;

public class GetKnowledgeBaseCategoryBySlugQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetKnowledgeBaseCategoryBySlugQuery, KnowledgeBaseCategoryDto?>
{

    public async Task<KnowledgeBaseCategoryDto?> Handle(GetKnowledgeBaseCategoryBySlugQuery request, CancellationToken cancellationToken)
    {
        var category = await context.Set<KnowledgeBaseCategory>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Slug == request.Slug && c.IsActive, cancellationToken);

        return category is not null ? mapper.Map<KnowledgeBaseCategoryDto>(category) : null;
    }
}
