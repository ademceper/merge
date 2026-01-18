using MediatR;
using Merge.Application.DTOs.Support;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Support.Commands.CreateFaq;

public record CreateFaqCommand(
    string Question,
    string Answer,
    string Category = "General",
    int SortOrder = 0,
    bool IsPublished = true
) : IRequest<FaqDto>;
