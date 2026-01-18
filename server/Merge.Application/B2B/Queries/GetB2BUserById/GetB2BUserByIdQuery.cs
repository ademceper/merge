using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Queries.GetB2BUserById;

public record GetB2BUserByIdQuery(Guid Id) : IRequest<B2BUserDto?>;

