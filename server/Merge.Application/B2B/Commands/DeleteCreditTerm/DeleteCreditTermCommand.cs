using MediatR;

namespace Merge.Application.B2B.Commands.DeleteCreditTerm;

public record DeleteCreditTermCommand(Guid Id) : IRequest<bool>;

