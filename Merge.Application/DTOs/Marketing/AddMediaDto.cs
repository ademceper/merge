namespace Merge.Application.DTOs.Marketing;

public class AddMediaDto
{
    public string Url { get; set; } = string.Empty;
    public string MediaType { get; set; } = "Photo";
    public string? ThumbnailUrl { get; set; }
}
