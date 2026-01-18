using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.DeleteQuestion;

public record DeleteQuestionCommand(
    Guid QuestionId,
    Guid UserId
) : IRequest<bool>;
