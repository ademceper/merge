using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.UnmarkAnswerHelpful;

public record UnmarkAnswerHelpfulCommand(
    Guid UserId,
    Guid AnswerId
) : IRequest;
