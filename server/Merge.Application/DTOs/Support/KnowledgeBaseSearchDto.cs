namespace Merge.Application.DTOs.Support;


public record KnowledgeBaseSearchDto(
    string Query,
    Guid? CategoryId,
    bool FeaturedOnly = false,
    int Page = 1,
    int PageSize = 20
);
