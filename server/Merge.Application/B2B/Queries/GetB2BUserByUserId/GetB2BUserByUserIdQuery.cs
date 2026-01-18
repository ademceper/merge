using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Queries.GetB2BUserByUserId;

public record GetB2BUserByUserIdQuery(Guid UserId) : IRequest<B2BUserDto?>;

