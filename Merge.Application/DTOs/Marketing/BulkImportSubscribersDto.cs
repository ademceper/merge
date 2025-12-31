namespace Merge.Application.DTOs.Marketing;

public class BulkImportSubscribersDto
{
    public List<CreateEmailSubscriberDto> Subscribers { get; set; } = new();
}
