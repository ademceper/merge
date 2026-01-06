using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Catalog;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Catalog.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger;
    private const string CACHE_KEY_ALL_CATEGORIES = "categories_all";
    private const string CACHE_KEY_MAIN_CATEGORIES = "categories_main";

    public UpdateCategoryCommandHandler(
        IRepository<Category> categoryRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IMapper mapper,
        ILogger<UpdateCategoryCommandHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating category with Id: {CategoryId}", request.Id);

        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException("Kategori", request.Id);
        }

        category.UpdateName(request.Name);
        category.UpdateDescription(request.Description);
        category.UpdateSlug(request.Slug);
        category.UpdateImageUrl(request.ImageUrl);
        category.SetParentCategory(request.ParentCategoryId);

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CACHE_KEY_ALL_CATEGORIES, cancellationToken);
        await _cache.RemoveAsync(CACHE_KEY_MAIN_CATEGORIES, cancellationToken);

        _logger.LogInformation("Category updated successfully with Id: {CategoryId}", request.Id);

        return _mapper.Map<CategoryDto>(category);
    }
}
