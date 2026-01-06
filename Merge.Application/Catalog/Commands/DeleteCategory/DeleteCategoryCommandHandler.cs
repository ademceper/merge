using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Catalog.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<DeleteCategoryCommandHandler> _logger;
    private const string CACHE_KEY_ALL_CATEGORIES = "categories_all";
    private const string CACHE_KEY_MAIN_CATEGORIES = "categories_main";

    public DeleteCategoryCommandHandler(
        IRepository<Category> categoryRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<DeleteCategoryCommandHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting category with Id: {CategoryId}", request.Id);

        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category == null)
        {
            _logger.LogWarning("Category not found with Id: {CategoryId}", request.Id);
            return false;
        }

        await _categoryRepository.DeleteAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CACHE_KEY_ALL_CATEGORIES, cancellationToken);
        await _cache.RemoveAsync(CACHE_KEY_MAIN_CATEGORIES, cancellationToken);

        _logger.LogInformation("Category deleted successfully with Id: {CategoryId}", request.Id);

        return true;
    }
}
