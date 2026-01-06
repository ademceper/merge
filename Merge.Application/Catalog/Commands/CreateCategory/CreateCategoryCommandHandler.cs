using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Catalog;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Catalog.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCategoryCommandHandler> _logger;
    private const string CACHE_KEY_ALL_CATEGORIES = "categories_all";
    private const string CACHE_KEY_MAIN_CATEGORIES = "categories_main";

    public CreateCategoryCommandHandler(
        IRepository<Category> categoryRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IMapper mapper,
        ILogger<CreateCategoryCommandHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating category with Name: {Name}, Slug: {Slug}", request.Name, request.Slug);

        var category = Category.Create(
            request.Name,
            request.Description,
            request.Slug,
            request.ImageUrl,
            request.ParentCategoryId);

        category = await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CACHE_KEY_ALL_CATEGORIES, cancellationToken);
        await _cache.RemoveAsync(CACHE_KEY_MAIN_CATEGORIES, cancellationToken);

        _logger.LogInformation("Category created successfully with Id: {CategoryId}", category.Id);

        return _mapper.Map<CategoryDto>(category);
    }
}
