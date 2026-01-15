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

namespace Merge.Application.International.Queries.GetLanguageByCode;

public class GetLanguageByCodeQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetLanguageByCodeQueryHandler> logger) : IRequestHandler<GetLanguageByCodeQuery, LanguageDto?>
{
    public async Task<LanguageDto?> Handle(GetLanguageByCodeQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting language by code. Code: {Code}", request.Code);

        var language = await context.Set<Language>()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Code.ToLower() == request.Code.ToLower(), cancellationToken);

        return language != null ? mapper.Map<LanguageDto>(language) : null;
    }
}
