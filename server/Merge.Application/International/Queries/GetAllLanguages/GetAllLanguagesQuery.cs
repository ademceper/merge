using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetAllLanguages;

public record GetAllLanguagesQuery() : IRequest<IEnumerable<LanguageDto>>;

