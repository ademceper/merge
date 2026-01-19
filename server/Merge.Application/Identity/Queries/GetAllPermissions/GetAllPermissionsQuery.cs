using MediatR;
using Merge.Application.DTOs.Identity;

namespace Merge.Application.Identity.Queries.GetAllPermissions;

public record GetAllPermissionsQuery(
    string? Category = null,
    string? Resource = null) : IRequest<List<PermissionDto>>;
