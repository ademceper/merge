using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetTranslationStats;

public record GetTranslationStatsQuery() : IRequest<TranslationStatsDto>;

