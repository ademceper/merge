using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetAllLanguages;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAllLanguagesQuery() : IRequest<IEnumerable<LanguageDto>>;

