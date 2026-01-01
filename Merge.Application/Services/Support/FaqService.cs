using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Support;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Support;
using Merge.Application.Common;


namespace Merge.Application.Services.Support;

public class FaqService : IFaqService
{
    private readonly IRepository<FAQ> _faqRepository;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FaqService> _logger;

    public FaqService(
        IRepository<FAQ> faqRepository,
        ApplicationDbContext context,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ILogger<FaqService> logger)
    {
        _faqRepository = faqRepository;
        _context = context;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<FaqDto?> GetByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Direct DbContext query for better control
        var faq = await _context.FAQs
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id);
        return faq == null ? null : _mapper.Map<FaqDto>(faq);
    }

    public async Task<IEnumerable<FaqDto>> GetAllAsync()
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !f.IsDeleted (Global Query Filter)
        var faqs = await _context.FAQs
            .AsNoTracking()
            .OrderBy(f => f.SortOrder)
            .ThenBy(f => f.Question)
            .ToListAsync();

        return _mapper.Map<IEnumerable<FaqDto>>(faqs);
    }

    public async Task<PagedResult<FaqDto>> GetAllAsync(int page, int pageSize)
    {
        var query = _context.FAQs
            .AsNoTracking()
            .OrderBy(f => f.SortOrder)
            .ThenBy(f => f.Question);

        var totalCount = await query.CountAsync();
        var faqs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<FaqDto>
        {
            Items = _mapper.Map<List<FaqDto>>(faqs),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<FaqDto>> GetByCategoryAsync(string category)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !f.IsDeleted (Global Query Filter)
        var faqs = await _context.FAQs
            .AsNoTracking()
            .Where(f => f.Category == category && f.IsPublished)
            .OrderBy(f => f.SortOrder)
            .ThenBy(f => f.Question)
            .ToListAsync();

        return _mapper.Map<IEnumerable<FaqDto>>(faqs);
    }

    public async Task<PagedResult<FaqDto>> GetByCategoryAsync(string category, int page, int pageSize)
    {
        var query = _context.FAQs
            .AsNoTracking()
            .Where(f => f.Category == category && f.IsPublished)
            .OrderBy(f => f.SortOrder)
            .ThenBy(f => f.Question);

        var totalCount = await query.CountAsync();
        var faqs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<FaqDto>
        {
            Items = _mapper.Map<List<FaqDto>>(faqs),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<FaqDto>> GetPublishedAsync()
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !f.IsDeleted (Global Query Filter)
        var faqs = await _context.FAQs
            .AsNoTracking()
            .Where(f => f.IsPublished)
            .OrderBy(f => f.SortOrder)
            .ThenBy(f => f.Question)
            .ToListAsync();

        return _mapper.Map<IEnumerable<FaqDto>>(faqs);
    }

    public async Task<PagedResult<FaqDto>> GetPublishedAsync(int page, int pageSize)
    {
        var query = _context.FAQs
            .AsNoTracking()
            .Where(f => f.IsPublished)
            .OrderBy(f => f.SortOrder)
            .ThenBy(f => f.Question);

        var totalCount = await query.CountAsync();
        var faqs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<FaqDto>
        {
            Items = _mapper.Map<List<FaqDto>>(faqs),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<FaqDto> CreateAsync(CreateFaqDto dto)
    {
        var faq = _mapper.Map<FAQ>(dto);
        faq = await _faqRepository.AddAsync(faq);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<FaqDto>(faq);
    }

    public async Task<FaqDto> UpdateAsync(Guid id, UpdateFaqDto dto)
    {
        var faq = await _faqRepository.GetByIdAsync(id);
        if (faq == null)
        {
            throw new NotFoundException("SSS", id);
        }

        faq.Question = dto.Question;
        faq.Answer = dto.Answer;
        faq.Category = dto.Category;
        faq.SortOrder = dto.SortOrder;
        faq.IsPublished = dto.IsPublished;

        await _faqRepository.UpdateAsync(faq);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<FaqDto>(faq);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var faq = await _faqRepository.GetByIdAsync(id);
        if (faq == null)
        {
            return false;
        }

        await _faqRepository.DeleteAsync(faq);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task IncrementViewCountAsync(Guid id)
    {
        var faq = await _faqRepository.GetByIdAsync(id);
        if (faq != null)
        {
            faq.ViewCount++;
            await _faqRepository.UpdateAsync(faq);
            // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
            await _unitOfWork.SaveChangesAsync();
        }
    }
}

