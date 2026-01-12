using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Commands.CreateBanner;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateBannerCommandHandler : IRequestHandler<CreateBannerCommand, BannerDto>
{
    private readonly Merge.Application.Interfaces.IRepository<Banner> _bannerRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateBannerCommandHandler> _logger;
    private const string CACHE_KEY_BANNER_BY_ID = "banner_";
    private const string CACHE_KEY_ACTIVE_BANNERS = "banners_active_";
    private const string CACHE_KEY_ALL_BANNERS = "banners_all_";

    public CreateBannerCommandHandler(
        Merge.Application.Interfaces.IRepository<Banner> bannerRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IMapper mapper,
        ILogger<CreateBannerCommandHandler> logger)
    {
        _bannerRepository = bannerRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BannerDto> Handle(CreateBannerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating banner. Title: {Title}, Position: {Position}",
            request.Title, request.Position);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var banner = Banner.Create(
                request.Title,
                request.ImageUrl,
                request.Position,
                request.Description,
                request.LinkUrl,
                request.SortOrder,
                request.IsActive,
                request.StartDate,
                request.EndDate,
                request.CategoryId,
                request.ProductId);

            banner = await _bannerRepository.AddAsync(banner, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Banner created successfully. BannerId: {BannerId}, Title: {Title}",
                banner.Id, banner.Title);

            // ✅ BOLUM 10.2: Cache invalidation - Remove all banner-related cache
            await _cache.RemoveAsync($"{CACHE_KEY_BANNER_BY_ID}{banner.Id}", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_ACTIVE_BANNERS}{request.Position}", cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ALL_BANNERS, cancellationToken);

            return _mapper.Map<BannerDto>(banner);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while creating banner. Title: {Title}, Position: {Position}",
                request.Title, request.Position);
            throw new BusinessException("Banner oluşturma çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error creating banner. Title: {Title}, Position: {Position}",
                request.Title, request.Position);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
