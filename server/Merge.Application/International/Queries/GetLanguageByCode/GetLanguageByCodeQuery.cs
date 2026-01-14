using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetLanguageByCode;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetLanguageByCodeQuery(string Code) : IRequest<LanguageDto?>;

