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

namespace Merge.Application.International.Queries.GetLanguageById;

public class GetLanguageByIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetLanguageByIdQueryHandler> logger) : IRequestHandler<GetLanguageByIdQuery, LanguageDto?>
{
    public async Task<LanguageDto?> Handle(GetLanguageByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting language by ID. LanguageId: {LanguageId}", request.Id);

        var language = await context.Set<Language>()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        return language != null ? mapper.Map<LanguageDto>(language) : null;
    }
}
