using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetActiveLanguages;

public record GetActiveLanguagesQuery() : IRequest<IEnumerable<LanguageDto>>;

