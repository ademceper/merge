using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.ApproveAnswer;

public record ApproveAnswerCommand(
    Guid AnswerId
) : IRequest<bool>;
