using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.LiveCommerce;

public class JoinStreamDto
{
    [StringLength(100)]
    public string? GuestId { get; set; }
}
