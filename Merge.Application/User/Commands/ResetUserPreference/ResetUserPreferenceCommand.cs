using MediatR;
using Merge.Application.DTOs.User;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.User.Commands.ResetUserPreference;

public record ResetUserPreferenceCommand(Guid UserId) : IRequest<UserPreferenceDto>;
