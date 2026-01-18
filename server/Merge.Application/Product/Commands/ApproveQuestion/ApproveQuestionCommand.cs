using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.ApproveQuestion;

public record ApproveQuestionCommand(
    Guid QuestionId
) : IRequest<bool>;
