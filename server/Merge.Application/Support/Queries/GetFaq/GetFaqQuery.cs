using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetFaq;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetFaqQuery(
    Guid FaqId
) : IRequest<FaqDto?>;
