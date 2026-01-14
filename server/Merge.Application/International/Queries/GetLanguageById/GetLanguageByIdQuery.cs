using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetLanguageById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetLanguageByIdQuery(Guid Id) : IRequest<LanguageDto?>;

