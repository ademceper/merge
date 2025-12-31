using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.Content;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Marketing;


namespace Merge.Application.Services.Content;

public class BannerService : IBannerService
{
    private readonly IRepository<Banner> _bannerRepository;
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BannerService(
        IRepository<Banner> bannerRepository,
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _bannerRepository = bannerRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BannerDto?> GetByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Direct DbContext query for better control
        var banner = await _context.Banners
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
        return banner == null ? null : _mapper.Map<BannerDto>(banner);
    }

    public async Task<IEnumerable<BannerDto>> GetAllAsync()
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !b.IsDeleted (Global Query Filter)
        var banners = await _context.Banners
            .AsNoTracking()
            .OrderBy(b => b.Position)
            .ThenBy(b => b.SortOrder)
            .ToListAsync();

        return _mapper.Map<IEnumerable<BannerDto>>(banners);
    }

    public async Task<IEnumerable<BannerDto>> GetActiveBannersAsync(string? position = null)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !b.IsDeleted (Global Query Filter)
        var now = DateTime.UtcNow;
        var query = _context.Banners
            .AsNoTracking()
            .Where(b => b.IsActive &&
                  (!b.StartDate.HasValue || b.StartDate.Value <= now) &&
                  (!b.EndDate.HasValue || b.EndDate.Value >= now));

        if (!string.IsNullOrEmpty(position))
        {
            query = query.Where(b => b.Position == position);
        }

        var banners = await query
            .OrderBy(b => b.SortOrder)
            .ToListAsync();

        return _mapper.Map<IEnumerable<BannerDto>>(banners);
    }

    public async Task<BannerDto> CreateAsync(CreateBannerDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        var banner = _mapper.Map<Banner>(dto);
        banner = await _bannerRepository.AddAsync(banner);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<BannerDto>(banner);
    }

    public async Task<BannerDto> UpdateAsync(Guid id, UpdateBannerDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        var banner = await _bannerRepository.GetByIdAsync(id);
        if (banner == null)
        {
            throw new NotFoundException("Banner", id);
        }

        banner.Title = dto.Title;
        banner.Description = dto.Description;
        banner.ImageUrl = dto.ImageUrl;
        banner.LinkUrl = dto.LinkUrl;
        banner.Position = dto.Position;
        banner.SortOrder = dto.SortOrder;
        banner.IsActive = dto.IsActive;
        banner.StartDate = dto.StartDate;
        banner.EndDate = dto.EndDate;
        banner.CategoryId = dto.CategoryId;
        banner.ProductId = dto.ProductId;

        await _bannerRepository.UpdateAsync(banner);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<BannerDto>(banner);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var banner = await _bannerRepository.GetByIdAsync(id);
        if (banner == null)
        {
            return false;
        }

        await _bannerRepository.DeleteAsync(banner);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}

