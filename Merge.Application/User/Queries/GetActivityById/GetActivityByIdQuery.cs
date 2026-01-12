using MediatR;
using Merge.Application.DTOs.User;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.User.Queries.GetActivityById;

public record GetActivityByIdQuery(Guid Id) : IRequest<UserActivityLogDto?>;
