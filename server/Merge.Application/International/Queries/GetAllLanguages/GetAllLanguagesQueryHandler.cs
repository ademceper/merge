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

namespace Merge.Application.International.Queries.GetAllLanguages;

public class GetAllLanguagesQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetAllLanguagesQueryHandler> logger) : IRequestHandler<GetAllLanguagesQuery, IEnumerable<LanguageDto>>
{
    public async Task<IEnumerable<LanguageDto>> Handle(GetAllLanguagesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting all languages");

        var languages = await context.Set<Language>()
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .Take(200)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<LanguageDto>>(languages);
    }
}
