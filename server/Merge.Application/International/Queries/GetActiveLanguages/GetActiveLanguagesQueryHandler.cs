using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Queries.GetActiveLanguages;

public class GetActiveLanguagesQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetActiveLanguagesQueryHandler> logger) : IRequestHandler<GetActiveLanguagesQuery, IEnumerable<LanguageDto>>
{
    public async Task<IEnumerable<LanguageDto>> Handle(GetActiveLanguagesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting active languages");

        var languages = await context.Set<Language>()
            .AsNoTracking()
            .Where(l => l.IsActive)
            .OrderBy(l => l.Name)
            .Take(100)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<LanguageDto>>(languages);
    }
}
