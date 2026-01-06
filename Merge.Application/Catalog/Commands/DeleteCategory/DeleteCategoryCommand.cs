using MediatR;

namespace Merge.Application.Catalog.Commands.DeleteCategory;

public record DeleteCategoryCommand(
    Guid Id
) : IRequest<bool>;
