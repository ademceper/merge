using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Queries.GetB2BUserByUserId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetB2BUserByUserIdQuery(Guid UserId) : IRequest<B2BUserDto?>;

