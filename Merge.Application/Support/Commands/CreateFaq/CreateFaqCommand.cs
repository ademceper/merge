using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Commands.CreateFaq;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateFaqCommand(
    string Question,
    string Answer,
    string Category = "General",
    int SortOrder = 0,
    bool IsPublished = true
) : IRequest<FaqDto>;
