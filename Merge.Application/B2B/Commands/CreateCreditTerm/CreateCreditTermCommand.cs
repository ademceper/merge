using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.CreateCreditTerm;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateCreditTermCommand(CreateCreditTermDto Dto) : IRequest<CreditTermDto>;

