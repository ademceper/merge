using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetActiveLanguages;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetActiveLanguagesQuery() : IRequest<IEnumerable<LanguageDto>>;

