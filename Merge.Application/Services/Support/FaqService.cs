using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.Support;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Support;


namespace Merge.Application.Services.Support;

public class FaqService : IFaqService
{
    private readonly IRepository<FAQ> _faqRepository;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public FaqService(
        IRepository<FAQ> faqRepository,
        ApplicationDbContext context,
        IMapper mapper)
    {
        _faqRepository = faqRepository;
        _context = context;
        _mapper = mapper;
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

    public async Task<FaqDto> CreateAsync(CreateFaqDto dto)
    {
        var faq = _mapper.Map<FAQ>(dto);
        faq = await _faqRepository.AddAsync(faq);
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
        return true;
    }

    public async Task IncrementViewCountAsync(Guid id)
    {
        var faq = await _faqRepository.GetByIdAsync(id);
        if (faq != null)
        {
            faq.ViewCount++;
            await _faqRepository.UpdateAsync(faq);
        }
    }
}

