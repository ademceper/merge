using MediatR;

namespace Merge.Application.B2B.Commands.DeleteCreditTerm;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteCreditTermCommand(Guid Id) : IRequest<bool>;

