using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Security;

public class ResolveAlertDto
{
    [StringLength(2000)]
    public string? ResolutionNotes { get; set; }
}
