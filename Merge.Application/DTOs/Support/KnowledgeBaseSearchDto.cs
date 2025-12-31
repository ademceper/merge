namespace Merge.Application.DTOs.Support;

public class KnowledgeBaseSearchDto
{
    public string Query { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public bool FeaturedOnly { get; set; } = false;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
