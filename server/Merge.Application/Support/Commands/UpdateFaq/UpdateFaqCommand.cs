using MediatR;
using Merge.Application.DTOs.Support;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Support.Commands.UpdateFaq;

public record UpdateFaqCommand(
    Guid FaqId,
    string Question,
    string Answer,
    string Category,
    int SortOrder,
    bool IsPublished
) : IRequest<FaqDto?>;
