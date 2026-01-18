using MediatR;

namespace Merge.Application.Support.Commands.DeleteFaq;

public record DeleteFaqCommand(
    Guid FaqId
) : IRequest<bool>;
