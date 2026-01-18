using System.ComponentModel.DataAnnotations;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.Auth;

public record LoginDto(
    [Required] [EmailAddress] string Email,
    [Required] string Password);

