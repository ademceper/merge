using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

public class BulkImportSubscribersDto
{
    [Required(ErrorMessage = "Subscribers list is required")]
    [MinLength(1, ErrorMessage = "At least one subscriber is required")]
    public List<CreateEmailSubscriberDto> Subscribers { get; set; } = new();
}
