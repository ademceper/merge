using MediatR;

namespace Merge.Application.Content.Commands.DeleteBlogCategory;

public record DeleteBlogCategoryCommand(Guid Id) : IRequest<bool>;

