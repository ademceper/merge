using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.CreateCreditTerm;

public record CreateCreditTermCommand(CreateCreditTermDto Dto) : IRequest<CreditTermDto>;

