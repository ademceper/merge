using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Security;

public class VerifyOrderDto
{
    [StringLength(2000)]
    public string? Notes { get; set; }
}
