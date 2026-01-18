using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.UnmarkQuestionHelpful;

public record UnmarkQuestionHelpfulCommand(
    Guid UserId,
    Guid QuestionId
) : IRequest;
