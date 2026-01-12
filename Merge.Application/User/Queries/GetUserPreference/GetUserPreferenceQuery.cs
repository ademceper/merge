using MediatR;
using Merge.Application.DTOs.User;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.User.Queries.GetUserPreference;

public record GetUserPreferenceQuery(Guid UserId) : IRequest<UserPreferenceDto>;
