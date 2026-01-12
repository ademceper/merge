using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Commands.UpdateFaq;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateFaqCommand(
    Guid FaqId,
    string Question,
    string Answer,
    string Category,
    int SortOrder,
    bool IsPublished
) : IRequest<FaqDto?>;
