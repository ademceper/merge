namespace Merge.Application.DTOs.Marketing;

public class ReviewMediaDto
{
    public Guid Id { get; set; }
    public string MediaType { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
}
