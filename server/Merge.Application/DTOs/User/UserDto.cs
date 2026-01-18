using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.User;

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Role);
