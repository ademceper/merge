using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.LiveCommerce;

public class LeaveStreamDto
{
    [StringLength(100)]
    public string? GuestId { get; set; }
}
