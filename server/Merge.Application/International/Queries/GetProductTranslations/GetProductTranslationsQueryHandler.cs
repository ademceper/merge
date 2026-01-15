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

namespace Merge.Application.International.Queries.GetProductTranslations;

public class GetProductTranslationsQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetProductTranslationsQueryHandler> logger) : IRequestHandler<GetProductTranslationsQuery, IEnumerable<ProductTranslationDto>>
{
    public async Task<IEnumerable<ProductTranslationDto>> Handle(GetProductTranslationsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting product translations. ProductId: {ProductId}", request.ProductId);

        var translations = await context.Set<ProductTranslation>()
            .AsNoTracking()
            .Where(pt => pt.ProductId == request.ProductId)
            .Take(50)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<ProductTranslationDto>>(translations);
    }
}
