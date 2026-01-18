using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.UpdateB2BUser;

public record UpdateB2BUserCommand(
    Guid Id,
    UpdateB2BUserDto Dto
) : IRequest<bool>;

