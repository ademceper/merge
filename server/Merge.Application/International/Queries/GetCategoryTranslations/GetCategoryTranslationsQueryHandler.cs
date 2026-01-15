using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Queries.GetCategoryTranslations;

public class GetCategoryTranslationsQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetCategoryTranslationsQueryHandler> logger) : IRequestHandler<GetCategoryTranslationsQuery, IEnumerable<CategoryTranslationDto>>
{
    public async Task<IEnumerable<CategoryTranslationDto>> Handle(GetCategoryTranslationsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting category translations. CategoryId: {CategoryId}", request.CategoryId);

        var translations = await context.Set<CategoryTranslation>()
            .AsNoTracking()
            .Where(ct => ct.CategoryId == request.CategoryId)
            .Take(50)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<CategoryTranslationDto>>(translations);
    }
}
