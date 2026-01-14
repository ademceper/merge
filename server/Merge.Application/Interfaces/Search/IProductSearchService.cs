using Merge.Application.DTOs.Search;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Search;

public interface IProductSearchService
{
    Task<SearchResultDto> SearchAsync(SearchRequestDto request, CancellationToken cancellationToken = default);
}

