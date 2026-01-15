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

namespace Merge.Application.International.Queries.GetProductTranslation;

public class GetProductTranslationQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetProductTranslationQueryHandler> logger) : IRequestHandler<GetProductTranslationQuery, ProductTranslationDto?>
{
    public async Task<ProductTranslationDto?> Handle(GetProductTranslationQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting product translation. ProductId: {ProductId}, LanguageCode: {LanguageCode}", 
            request.ProductId, request.LanguageCode);

        var translation = await context.Set<ProductTranslation>()
            .AsNoTracking()
            .FirstOrDefaultAsync(pt => pt.ProductId == request.ProductId &&
                                      pt.LanguageCode.ToLower() == request.LanguageCode.ToLower(), cancellationToken);

        return translation != null ? mapper.Map<ProductTranslationDto>(translation) : null;
    }
}
