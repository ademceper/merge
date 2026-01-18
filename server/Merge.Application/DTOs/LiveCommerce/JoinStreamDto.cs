using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.LiveCommerce;

public record JoinStreamDto(
    [StringLength(100)] string? GuestId);
