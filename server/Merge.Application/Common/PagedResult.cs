using System.Text.Json.Serialization;

namespace Merge.Application.Common;

/// <summary>
/// Paginated result with HATEOAS support
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
    
    [JsonPropertyName("_links")]
    public Dictionary<string, object>? Links { get; set; }
}
