using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.LiveCommerce;

public record LeaveStreamDto(
    [StringLength(100)] string? GuestId);
