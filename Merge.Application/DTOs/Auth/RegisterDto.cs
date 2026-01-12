using System.ComponentModel.DataAnnotations;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.Auth;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record RegisterDto(
    [Required] [StringLength(100)] string FirstName,
    [Required] [StringLength(100)] string LastName,
    [Required] [EmailAddress] string Email,
    [Required] [MinLength(6)] string Password,
    [Required] [Phone] string PhoneNumber);

