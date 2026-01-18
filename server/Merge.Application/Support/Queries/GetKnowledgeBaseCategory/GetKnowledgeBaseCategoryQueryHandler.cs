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

namespace Merge.Application.Support.Queries.GetKnowledgeBaseCategory;

public class GetKnowledgeBaseCategoryQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetKnowledgeBaseCategoryQuery, KnowledgeBaseCategoryDto?>
{

    public async Task<KnowledgeBaseCategoryDto?> Handle(GetKnowledgeBaseCategoryQuery request, CancellationToken cancellationToken)
    {
        var category = await context.Set<KnowledgeBaseCategory>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

        return category is not null ? mapper.Map<KnowledgeBaseCategoryDto>(category) : null;
    }
}
