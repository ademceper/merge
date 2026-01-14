using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Queries.GetB2BUserById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetB2BUserByIdQuery(Guid Id) : IRequest<B2BUserDto?>;

