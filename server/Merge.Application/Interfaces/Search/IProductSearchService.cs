using Merge.Application.DTOs.Search;

namespace Merge.Application.Interfaces.Search;

public interface IProductSearchService
{
    Task<SearchResultDto> SearchAsync(SearchRequestDto request, CancellationToken cancellationToken = default);
}

