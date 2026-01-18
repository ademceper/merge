using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Queries.GetCreditTermById;

public record GetCreditTermByIdQuery(Guid Id) : IRequest<CreditTermDto?>;

