using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.MarkQuestionHelpful;

public record MarkQuestionHelpfulCommand(
    Guid UserId,
    Guid QuestionId
) : IRequest;
